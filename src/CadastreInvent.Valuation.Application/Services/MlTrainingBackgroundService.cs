using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.ML;
using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Application.Services
{
    public class MlTrainingBackgroundService : BackgroundService
    {
        private readonly IMlTrainingQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMassAppraisalDiagnosticLogger _logger;

        public MlTrainingBackgroundService(
            IMlTrainingQueue queue,
            IServiceProvider serviceProvider,
            IMassAppraisalDiagnosticLogger logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInfo("ML Training Worker", "Фоновый процесс ожидания команд запущен.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var jobContext = await _queue.DequeueJobAsync(stoppingToken);
                    await ProcessTrainingAsync(jobContext, stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError("ML Training Worker", "Глобальный сбой воркера обучения", ex);
                }
            }
        }

        private async Task ProcessTrainingAsync(DTOs.MlTrainingJobContext context, CancellationToken stoppingToken)
        {
            _logger.LogInfo("ML Training Pipeline", $"Начат сбор данных для модели {context.Version}...");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CadastreDbContext>();
            var mlService = scope.ServiceProvider.GetRequiredService<IMassAppraisalMLService>();
            var spatialFeatureService = scope.ServiceProvider.GetRequiredService<ISpatialFeatureService>();

            try
            {
                var validTransactions = await dbContext.SalesTransactions
                    .AsNoTracking()
                    .Where(t => t.Validity == TransactionValidity.ValidMarket)
                    .ToListAsync(stoppingToken);

                if (validTransactions.Count < 10)
                {
                    await SaveFailedModelAsync(dbContext, context, "Failed_InsufficientData", $"Найдено всего {validTransactions.Count} достоверных сделок. Нужно минимум 10.", stoppingToken);
                    return;
                }

                var unitIds = validTransactions.Select(t => t.ValuationUnitId).Distinct().ToList();
                var valuationUnits = await dbContext.ValuationUnits.AsNoTracking().Where(u => unitIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, stoppingToken);
                var characteristics = await dbContext.PropertyCharacteristics.AsNoTracking().Where(c => unitIds.Contains(c.ValuationUnitId)).ToDictionaryAsync(c => c.ValuationUnitId, stoppingToken);
                var spatialMetrics = await spatialFeatureService.GetSpatialMetricsAsync(unitIds, stoppingToken);

                var trainingDataWithIds = new List<(UnifiedValuationVector Vector, Guid TransactionId)>();

                foreach (var transaction in validTransactions)
                {
                    if (!valuationUnits.TryGetValue(transaction.ValuationUnitId, out var unit) ||
                        !characteristics.TryGetValue(transaction.ValuationUnitId, out var characteristic)) continue;

                    try
                    {
                        float area = 0, year = 1950, floor = 1, dist = 10, rooms = 1;

                        using (var doc = JsonDocument.Parse(characteristic.CharacteristicsJson))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("Area", out var a) && a.ValueKind == JsonValueKind.Number) area = a.GetSingle();
                            if (root.TryGetProperty("YearBuilt", out var y) && y.ValueKind == JsonValueKind.Number) year = y.GetSingle();
                            if (root.TryGetProperty("Floor", out var f) && f.ValueKind == JsonValueKind.Number) floor = f.GetSingle();
                            if (root.TryGetProperty("DistanceToCenterKm", out var d) && d.ValueKind == JsonValueKind.Number) dist = d.GetSingle();
                            if (root.TryGetProperty("RoomsCount", out var r) && r.ValueKind == JsonValueKind.Number) rooms = r.GetSingle();
                        }

                        if (spatialMetrics.TryGetValue(transaction.ValuationUnitId, out var metrics))
                        {
                            if (metrics.ActualAreaSqMeters > 0) area = (float)metrics.ActualAreaSqMeters;
                            if (metrics.DistanceToCenterKm > 0) dist = (float)metrics.DistanceToCenterKm;
                        }

                        if (area <= 0) continue;

                        var vector = new UnifiedValuationVector
                        {
                            Price = (float)transaction.SalePrice,
                            AreaSqMeters = area,
                            YearBuilt = year,
                            Floor = floor,
                            DistanceToCenterKm = dist,
                            RoomsCount = rooms,
                            ZoningCode = unit.ZoningStatus,
                            HasViolations = characteristic.HasViolations
                        };

                        trainingDataWithIds.Add((vector, transaction.Id));
                    }
                    catch { }
                }

                if (trainingDataWithIds.Count < 10)
                {
                    await SaveFailedModelAsync(dbContext, context, "Failed_CorruptedJson", "Данные повреждены (площадь равна нулю или ошибка JSON).", stoppingToken);
                    return;
                }

                var pricesPerSqM = trainingDataWithIds.Select(x => x.Vector.Price / x.Vector.AreaSqMeters).OrderBy(x => x).ToList();
                double q1 = pricesPerSqM[(int)(pricesPerSqM.Count * 0.25)];
                double q3 = pricesPerSqM[(int)(pricesPerSqM.Count * 0.75)];
                double iqr = q3 - q1;
                double lowerBound = q1 - 1.5 * iqr;
                double upperBound = q3 + 1.5 * iqr;

                var cleanDataWithIds = trainingDataWithIds.Where(x =>
                {
                    double psm = x.Vector.Price / x.Vector.AreaSqMeters;
                    return psm >= lowerBound && psm <= upperBound;
                }).ToList();

                int outliersCount = trainingDataWithIds.Count - cleanDataWithIds.Count;
                if (outliersCount > 0)
                {
                    _logger.LogWarning("ML Training Pipeline", $"IQR-фильтр отбросил {outliersCount} выбросов из оперативной памяти. В базе они остались нетронутыми.");
                }

                var trainingData = cleanDataWithIds.Select(x => x.Vector).ToList();

                if (trainingData.Count < 10)
                {
                    await SaveFailedModelAsync(dbContext, context, "Failed_OutliersDetected", "После очистки от выбросов осталось менее 10 записей.", stoppingToken);
                    return;
                }

                _logger.LogInfo("ML Training Pipeline", $"Обучение Sdca начато на {trainingData.Count} чистых записях...");
                var mlResult = mlService.TrainModel(trainingData);

                string finalStatus = "Active";
                string finalDescription = context.Description;

                if (mlResult.RSquared < 0.70 || mlResult.Mape > 0.30)
                {
                    finalStatus = "TrainingFailed_LowAccuracy";
                    finalDescription = $"Сработали Врата Качества. Точность (R²) = {(mlResult.RSquared * 100):F1}%, Ошибка (MAPE) = {(mlResult.Mape * 100):F1}%. Модель отбракована.";
                    _logger.LogWarning("ML Training Pipeline", finalDescription);
                }

                var newModel = new MassAppraisalModel(context.Version, finalDescription, "SdcaRegression", DateTime.UtcNow);
                dbContext.Entry(newModel).Property("Id").CurrentValue = Guid.NewGuid(); 
                newModel.SetTrainedModel(mlResult.ModelBytes, JsonSerializer.Serialize(new { RSquared = mlResult.RSquared, Mape = mlResult.Mape, Cod = mlResult.Cod }), finalStatus);

                dbContext.MassAppraisalModels.Add(newModel);
                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInfo("ML Training Pipeline", $"Модель {context.Version} успешно сохранена в БД со статусом {finalStatus}.");
            }
            catch (Exception ex)
            {
                _logger.LogError("ML Training Pipeline", "Критический сбой во время обучения", ex);
            }
        }

        private async Task SaveFailedModelAsync(CadastreDbContext dbContext, DTOs.MlTrainingJobContext context, string status, string explanation, CancellationToken ct)
        {
            try
            {
                var failedModel = new MassAppraisalModel(context.Version, explanation, "SdcaRegression", DateTime.UtcNow);
                dbContext.Entry(failedModel).Property("Id").CurrentValue = Guid.NewGuid(); 
                failedModel.SetTrainedModel(Array.Empty<byte>(), "{}", status);

                dbContext.MassAppraisalModels.Add(failedModel);
                await dbContext.SaveChangesAsync(ct);
                _logger.LogInfo("ML Training Pipeline", $"Сохранен отказ модели со статусом: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError("ML Training Pipeline", "КРИТИЧЕСКИЙ СБОЙ: Невозможно сохранить статус Failed в БД.", ex);
            }
        }
    }
}