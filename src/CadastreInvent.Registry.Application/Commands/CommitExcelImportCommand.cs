using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Domain.Enums;
using CadastreInvent.Infrastructure.Services.Excel;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Registry.Application.Commands
{
    // ДОБАВЛЕН ПАРАМЕТР UpdateDuplicates
    public record CommitExcelImportCommand(Guid SessionId, bool UpdateDuplicates) : IRequest<int>;

    public class CommitExcelImportCommandHandler : IRequestHandler<CommitExcelImportCommand, int>
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private readonly ICurrentUserService _currentUserService;

        public CommitExcelImportCommandHandler(CadastreDbContext dbContext, IDistributedCache cache, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _cache = cache;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(CommitExcelImportCommand request, CancellationToken cancellationToken)
        {
            var cacheKey = $"ExcelImportSession_{request.SessionId}";
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(cachedData))
            {
                throw new InvalidOperationException("Сессия импорта истекла или не существует. Пожалуйста, загрузите файл заново.");
            }

            var sessionData = JsonSerializer.Deserialize<ExcelPreviewResultDto>(cachedData);
            if (sessionData == null || !sessionData.PreviewData.Any()) return 0;

            var validRows = sessionData.PreviewData.Where(r => r.IsValid).ToList();

            if (!request.UpdateDuplicates)
            {
                validRows = validRows.Where(r => !r.IsDuplicate).ToList();
            }

            if (!validRows.Any()) return 0;

            int importedCount = 0;
            int updatedCount = 0;
            var wktReader = new WKTReader();

            var userId = _currentUserService.UserId;
            var userName = "Неизвестный пользователь";

            if (userId != Guid.Empty)
            {
                var user = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user != null) userName = user.Email ?? "Пользователь";
            }

            var cadNumbers = validRows.Select(r => r.CadastralNumber).Distinct().ToList();

            var existingUnits = await _dbContext.SpatialUnits
                .Where(s => cadNumbers.Contains(s.ReferenceNumber))
                .ToDictionaryAsync(s => s.ReferenceNumber, cancellationToken);

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    foreach (var row in validRows)
                    {
                        var geometry = wktReader.Read(row.Wkt);
                        if (geometry is not Polygon polygon) continue;

                        polygon.SRID = 4326;

                        if (existingUnits.TryGetValue(row.CadastralNumber, out var existingUnit))
                        {
                            var entry = _dbContext.Entry(existingUnit);
                            entry.Property("Boundary").CurrentValue = polygon;
                            entry.Property("AreaSqMeters").CurrentValue = row.AreaSqMeters;
                            entry.Property("Type").CurrentValue = row.Type;

                            updatedCount++;
                        }
                        else
                        {
                            var spatialUnit = new SpatialUnit(row.CadastralNumber, row.Type, polygon, row.AreaSqMeters);
                            var baUnit = new BAUnit(row.Address, BAUnitType.BasicPropertyUnit);

                            baUnit.AddSpatialUnit(spatialUnit.Id);

                            _dbContext.SpatialUnits.Add(spatialUnit);
                            _dbContext.BAUnits.Add(baUnit);

                            existingUnits[row.CadastralNumber] = spatialUnit;
                            importedCount++;
                        }
                    }

                    var totalProcessed = importedCount + updatedCount;

                    var historyRecord = new ImportHistory(
                        sessionData.FileName,
                        sessionData.TotalRows,
                        totalProcessed,
                        userId,
                        userName
                    );
                    _dbContext.ImportHistories.Add(historyRecord);

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    await _cache.RemoveAsync(cacheKey, cancellationToken);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });

            return importedCount + updatedCount;
        }
    }
}