using System;
using Microsoft.ML.Data;

namespace CadastreInvent.Valuation.Application.ML
{
    public class UnifiedValuationVector
    {
        [LoadColumn(0)]
        [ColumnName("Label")]
        public float Price { get; set; }

        [LoadColumn(1)]
        public float AreaSqMeters { get; set; }

        [LoadColumn(2)]
        public float YearBuilt { get; set; }

        [LoadColumn(3)]
        public float Floor { get; set; }

        [LoadColumn(4)]
        public float DistanceToCenterKm { get; set; }

        [LoadColumn(5)]
        public float RoomsCount { get; set; }

        [LoadColumn(6)]
        public string ZoningCode { get; set; } = string.Empty;

        [LoadColumn(7)]
        public bool HasViolations { get; set; }
    }

    public class ValuationPrediction
    {
        [ColumnName("Score")]
        public float PredictedValue { get; set; }
    }

    public class MLTrainingResult
    {
        public byte[] ModelBytes { get; set; } = Array.Empty<byte>();
        public RegressionMetrics Metrics { get; set; } = null!;
        public double RSquared { get; set; }
        public double Mape { get; set; }
        public double Cod { get; set; }
    }
}