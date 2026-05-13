using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.ObjectPool;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CadastreInvent.Valuation.Application.ML
{
    public interface IMassAppraisalMLService
    {
        ValuationPrediction? PredictValue(UnifiedValuationVector data);
        string GetModelVersion();
        MLTrainingResult TrainModel(IEnumerable<UnifiedValuationVector> trainingData);
        void LoadModelFromBytes(byte[] modelBytes, string version);
    }

    public class MassAppraisalMLService : IMassAppraisalMLService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _activeModel;
        private string _activeVersion;
        private ObjectPool<PredictionEngine<UnifiedValuationVector, ValuationPrediction>>? _enginePool;

        public MassAppraisalMLService()
        {
            _mlContext = new MLContext(seed: 2026);
            _activeVersion = "NOT_LOADED";
        }

        public void LoadModelFromBytes(byte[] modelBytes, string version)
        {
            if (modelBytes == null || modelBytes.Length == 0) return;

            using var stream = new MemoryStream(modelBytes);
            var newModel = _mlContext.Model.Load(stream, out var schema);

            var provider = new DefaultObjectPoolProvider();
            var policy = new PredictionEnginePolicy(_mlContext, newModel);
            var newPool = provider.Create(policy);

            Interlocked.Exchange(ref _enginePool, newPool);
            _activeModel = newModel;
            _activeVersion = version;
        }

        public MLTrainingResult TrainModel(IEnumerable<UnifiedValuationVector> trainingData)
        {
            var dataList = trainingData.ToList();
            var dataView = _mlContext.Data.LoadFromEnumerable(dataList);

            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("ZoningCodeEncoded", nameof(UnifiedValuationVector.ZoningCode))
                .Append(_mlContext.Transforms.Conversion.ConvertType("HasViolationsEncoded", nameof(UnifiedValuationVector.HasViolations), DataKind.Single))
                .Append(_mlContext.Transforms.Concatenate("FeaturesUnnormalized", // Временно склеиваем как ненормализованные
                    nameof(UnifiedValuationVector.AreaSqMeters),
                    nameof(UnifiedValuationVector.YearBuilt),
                    nameof(UnifiedValuationVector.Floor),
                    nameof(UnifiedValuationVector.DistanceToCenterKm),
                    nameof(UnifiedValuationVector.RoomsCount),
                    "ZoningCodeEncoded",
                    "HasViolationsEncoded"))
                // ФИКС МАТЕМАТИКИ: Линейные алгоритмы требуют нормализации (MinMax), иначе они выпадают в NaN (Not a Number)!
                .Append(_mlContext.Transforms.NormalizeMinMax("Features", "FeaturesUnnormalized"))
                .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));

            var cvResults = _mlContext.Regression.CrossValidate(dataView, pipeline, numberOfFolds: 5, labelColumnName: "Label");

            var validFolds = cvResults.Where(r => !double.IsInfinity(r.Metrics.RSquared) && !double.IsNaN(r.Metrics.RSquared) && r.Metrics.RSquared <= 1.0).ToList();
            var rSquared = validFolds.Any() ? validFolds.Average(r => r.Metrics.RSquared) : 0;

            var trainedModel = pipeline.Fit(dataView);

            var actualPrices = dataList.Select(x => x.Price).OrderBy(x => x).ToList();
            double medianPrice = actualPrices.Count > 0 ? actualPrices[actualPrices.Count / 2] : 0;

            var predEngine = _mlContext.Model.CreatePredictionEngine<UnifiedValuationVector, ValuationPrediction>(trainedModel);

            double sumPercentageError = 0;
            double sumAbsoluteDeviationFromMedian = 0;

            foreach (var item in dataList)
            {
                var pred = predEngine.Predict(item);
                if (item.Price > 0)
                {
                    sumPercentageError += Math.Abs((item.Price - pred.PredictedValue) / item.Price);
                }
                sumAbsoluteDeviationFromMedian += Math.Abs(pred.PredictedValue - medianPrice);
            }

            double mape = dataList.Count > 0 ? sumPercentageError / dataList.Count : 0;
            double cod = medianPrice > 0 ? (sumAbsoluteDeviationFromMedian / dataList.Count) / medianPrice : 0;

            using var stream = new MemoryStream();
            _mlContext.Model.Save(trainedModel, dataView.Schema, stream);

            return new MLTrainingResult
            {
                ModelBytes = stream.ToArray(),
                Metrics = validFolds.FirstOrDefault()?.Metrics ?? cvResults.First().Metrics,
                RSquared = rSquared,
                Mape = mape,
                Cod = cod
            };
        }

        public ValuationPrediction? PredictValue(UnifiedValuationVector data)
        {
            var pool = _enginePool;
            if (pool == null) return null;

            var engine = pool.Get();
            try { return engine.Predict(data); }
            finally { pool.Return(engine); }
        }

        public string GetModelVersion() => _activeVersion;

        private class PredictionEnginePolicy : PooledObjectPolicy<PredictionEngine<UnifiedValuationVector, ValuationPrediction>>
        {
            private readonly MLContext _mlContext;
            private readonly ITransformer _model;

            public PredictionEnginePolicy(MLContext mlContext, ITransformer model)
            {
                _mlContext = mlContext;
                _model = model;
            }

            public override PredictionEngine<UnifiedValuationVector, ValuationPrediction> Create() => _mlContext.Model.CreatePredictionEngine<UnifiedValuationVector, ValuationPrediction>(_model);
            public override bool Return(PredictionEngine<UnifiedValuationVector, ValuationPrediction> obj) => true;
        }
    }
}