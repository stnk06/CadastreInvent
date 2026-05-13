using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.DTOs;
using CadastreInvent.Valuation.Application.ML;
using CadastreInvent.Valuation.Application.Handlers;
using CadastreInvent.Valuation.Domain.Enums;
using ValuationEntity = CadastreInvent.Valuation.Domain.Entities.Valuation;

namespace CadastreInvent.Valuation.Application.Services
{
    public class MassAppraisalBackgroundService : BackgroundService
    {
        private readonly IMassAppraisalQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMassAppraisalNotificationService _notificationService;
        private readonly IMassAppraisalDiagnosticLogger _diagnosticLogger;
        private readonly ILogger<MassAppraisalBackgroundService> _logger;

        public MassAppraisalBackgroundService(
            IMassAppraisalQueue queue,
            IServiceProvider serviceProvider,
            IMassAppraisalNotificationService notificationService,
            IMassAppraisalDiagnosticLogger diagnosticLogger,
            ILogger<MassAppraisalBackgroundService> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _notificationService = notificationService;
            _diagnosticLogger = diagnosticLogger;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _diagnosticLogger.LogInfo("CAMA Engine", "Служба массовой оценки активирована и ожидает задачи...");
            var batchTask = ProcessBatchJobsAsync(stoppingToken);
            var singleTask = ProcessSingleRequestsAsync(stoppingToken);
            return Task.WhenAll(batchTask, singleTask);
        }

        private async Task ProcessSingleRequestsAsync(CancellationToken stoppingToken)
        {
            await foreach (var request in _queue.ReadAllSingleAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<CadastreDbContext>();
                    var mlService = scope.ServiceProvider.GetRequiredService<IMassAppraisalMLService>();
                    var spatialFeatureService = scope.ServiceProvider.GetRequiredService<ISpatialFeatureService>();

                    var baUnit = await dbContext.BAUnits.AsNoTracking().FirstOrDefaultAsync(b => b.SpatialUnits.Any(su => su.SpatialUnitId == request.SpatialUnitId), stoppingToken);
                    if (baUnit == null) continue;

                    var valuationUnit = await dbContext.ValuationUnits.AsNoTracking().FirstOrDefaultAsync(v => v.BAUnitId == baUnit.Id, stoppingToken);
                    if (valuationUnit == null) continue;

                    var characteristic = await dbContext.PropertyCharacteristics.AsNoTracking().FirstOrDefaultAsync(c => c.ValuationUnitId == valuationUnit.Id, stoppingToken);
                    if (characteristic == null) continue;

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

                    var spatialMetrics = await spatialFeatureService.GetSpatialMetricsAsync(new[] { valuationUnit.Id }, stoppingToken);
                    if (spatialMetrics.TryGetValue(valuationUnit.Id, out var metrics))
                    {
                        if (metrics.ActualAreaSqMeters > 0) area = (float)metrics.ActualAreaSqMeters;
                        if (metrics.DistanceToCenterKm > 0) dist = (float)metrics.DistanceToCenterKm;
                    }

                    if (area <= 0) continue;

                    var vector = new UnifiedValuationVector
                    {
                        AreaSqMeters = area,
                        YearBuilt = year,
                        Floor = floor,
                        DistanceToCenterKm = dist,
                        RoomsCount = rooms,
                        ZoningCode = valuationUnit.ZoningStatus,
                        HasViolations = characteristic.HasViolations
                    };

                    var prediction = mlService.PredictValue(vector);
                    if (prediction == null) continue;

                    decimal assessedValue = (decimal)prediction.PredictedValue;
                    if (assessedValue < 0) assessedValue = 0;

                    var activeModel = await dbContext.MassAppraisalModels.AsNoTracking().Where(x => !EF.Property<bool>(x, "IsDeleted")).OrderByDescending(m => m.CreatedAt).FirstOrDefaultAsync(stoppingToken);

                    // Одиночный инсерт не требует явной транзакции (EF Core обернет сам)
                    await dbContext.Valuations
                        .Where(v => v.ValuationUnitId == valuationUnit.Id && v.IsDeleted == false)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true), stoppingToken);

                    var newValuation = new ValuationEntity(
                        valuationUnit.Id,
                        Math.Round(assessedValue, 2),
                        DateTime.UtcNow,
                        ValuationMethod.AutomatedMachineLearning,
                        activeModel?.Id
                    );

                    dbContext.Valuations.Add(newValuation);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _diagnosticLogger.LogError("CAMA Engine (Single)", "Ошибка при одиночной ML-оценке.", ex);
                }
            }
        }

        private async Task ProcessBatchJobsAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var jobContext = await _queue.DequeueJobAsync(stoppingToken);
                    await ProcessMassAppraisalEngineAsync(jobContext, stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _diagnosticLogger.LogError("CAMA Engine", "Глобальный сбой пакетной обработки оценки.", ex);
                    _queue.UpdateProgress(0, 0, false);
                }
            }
        }

        private async Task ProcessMassAppraisalEngineAsync(MassAppraisalJobContext context, CancellationToken stoppingToken)
        {
            _diagnosticLogger.LogInfo("CAMA Engine", "Инициализация процесса массовой оценки...");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CadastreDbContext>();
            var mlService = scope.ServiceProvider.GetRequiredService<IMassAppraisalMLService>();
            var spatialFeatureService = scope.ServiceProvider.GetRequiredService<ISpatialFeatureService>();

            var model = await dbContext.MassAppraisalModels.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == context.ModelId, stoppingToken);
            if (model == null)
            {
                _diagnosticLogger.LogError("CAMA Engine", $"Модель с ID {context.ModelId} не найдена. Отмена.", null);
                return;
            }

            if (mlService.GetModelVersion() == "NOT_LOADED" || mlService.GetModelVersion() != model.Version)
            {
                _diagnosticLogger.LogWarning("CAMA Engine", "Горячая загрузка модели в оперативную память...");
                mlService.LoadModelFromBytes(model.ModelData, model.Version);
                _diagnosticLogger.LogInfo("CAMA Engine", $"Модель {model.Version} загружена.");
            }

            int batchSize = 1000;
            int skip = 0;
            bool hasMoreData = true;
            int processedCount = 0;

            var baseQuery = dbContext.ValuationUnits.AsNoTracking().Where(u => u.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(context.ZoningStatusFilter))
            {
                baseQuery = baseQuery.Where(u => u.ZoningStatus == context.ZoningStatusFilter);
            }

            int totalUnits = await baseQuery.CountAsync(stoppingToken);
            _queue.UpdateProgress(totalUnits, 0, true);
            await _notificationService.NotifyProgressAsync(0, totalUnits, "processing");

            if (totalUnits == 0)
            {
                _diagnosticLogger.LogWarning("CAMA Engine", "Для оценки не найдено ни одного подходящего объекта.");
                _queue.UpdateProgress(0, 0, false);
                await _notificationService.NotifyProgressAsync(0, 0, "completed");
                return;
            }

            _diagnosticLogger.LogInfo("CAMA Engine", $"Найдено объектов: {totalUnits}. Запуск нейросети...");

            while (hasMoreData && !stoppingToken.IsCancellationRequested)
            {
                var unitsBatch = await baseQuery.OrderBy(u => u.Id).Skip(skip).Take(batchSize).ToListAsync(stoppingToken);
                if (unitsBatch.Count == 0)
                {
                    hasMoreData = false;
                    continue;
                }

                var unitIds = unitsBatch.Select(u => u.Id).ToList();
                var characteristics = await dbContext.PropertyCharacteristics
                    .AsNoTracking()
                    .Where(c => unitIds.Contains(c.ValuationUnitId) && c.IsDeleted == false)
                    .ToDictionaryAsync(c => c.ValuationUnitId, stoppingToken);

                var spatialMetrics = await spatialFeatureService.GetSpatialMetricsAsync(unitIds, stoppingToken);

                var valuationsToInsert = new List<ValuationEntity>();
                var unitsToDeactivate = new List<Guid>();

                foreach (var unit in unitsBatch)
                {
                    if (!characteristics.TryGetValue(unit.Id, out var characteristic)) continue;

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

                        if (spatialMetrics.TryGetValue(unit.Id, out var metrics))
                        {
                            if (metrics.ActualAreaSqMeters > 0) area = (float)metrics.ActualAreaSqMeters;
                            if (metrics.DistanceToCenterKm > 0) dist = (float)metrics.DistanceToCenterKm;
                        }

                        if (area <= 0) continue;
                        if (context.MinArea.HasValue && area < context.MinArea.Value) continue;
                        if (context.MaxArea.HasValue && area > context.MaxArea.Value) continue;

                        var vector = new UnifiedValuationVector
                        {
                            AreaSqMeters = area,
                            YearBuilt = year,
                            Floor = floor,
                            DistanceToCenterKm = dist,
                            RoomsCount = rooms,
                            ZoningCode = unit.ZoningStatus,
                            HasViolations = characteristic.HasViolations
                        };

                        var prediction = mlService.PredictValue(vector);
                        if (prediction == null) continue;

                        decimal assessedValue = (decimal)prediction.PredictedValue;
                        if (assessedValue < 0) assessedValue = 0;

                        var valuation = new ValuationEntity(
                            unit.Id,
                            Math.Round(assessedValue, 2),
                            DateTime.UtcNow,
                            ValuationMethod.AutomatedMachineLearning,
                            context.ModelId
                        );

                        unitsToDeactivate.Add(unit.Id);
                        valuationsToInsert.Add(valuation);
                    }
                    catch { }
                }

                if (valuationsToInsert.Any())
                {
                    // ИСПРАВЛЕНИЕ: Инкапсулируем логику транзакции в ExecutionStrategy для совместимости с EnableRetryOnFailure
                    var executionStrategy = dbContext.Database.CreateExecutionStrategy();

                    await executionStrategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
                        try
                        {
                            // Очищаем ChangeTracker перед стартом, чтобы в случае Retry избежать дублирования сущностей
                            dbContext.ChangeTracker.Clear();

                            await dbContext.Valuations
                                .Where(v => unitsToDeactivate.Contains(v.ValuationUnitId) && v.IsDeleted == false)
                                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true), stoppingToken);

                            dbContext.Valuations.AddRange(valuationsToInsert);
                            await dbContext.SaveChangesAsync(stoppingToken);

                            await transaction.CommitAsync(stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync(stoppingToken);
                            dbContext.ChangeTracker.Clear();
                            _diagnosticLogger.LogWarning("CAMA Engine", $"Отклонение транзакции: {ex.Message}. Ожидание автоматического повтора (Retry)...");
                            throw; // Пробрасываем ошибку дальше, чтобы ExecutionStrategy мог запустить Retry
                        }
                    });

                    processedCount += valuationsToInsert.Count;
                    _diagnosticLogger.LogInfo("CAMA Engine", $"Сохранен батч из {valuationsToInsert.Count} оценок. Всего обработано: {processedCount} / {totalUnits}.");
                }

                skip += batchSize;
                _queue.UpdateProgress(totalUnits, processedCount, true);
                await _notificationService.NotifyProgressAsync(processedCount, totalUnits, "processing");
                dbContext.ChangeTracker.Clear();
            }

            await Task.Delay(1500, stoppingToken);
            _queue.UpdateProgress(totalUnits, processedCount, false);
            await _notificationService.NotifyProgressAsync(processedCount, totalUnits, "completed");

            _diagnosticLogger.LogInfo("CAMA Engine", "✅ Процесс массовой оценки полностью и успешно завершен!");
        }
    }
}