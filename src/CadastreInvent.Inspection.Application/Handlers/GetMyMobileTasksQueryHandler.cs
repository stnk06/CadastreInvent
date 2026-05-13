using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Inspection.Application.DTOs;
using CadastreInvent.Inspection.Application.Queries;
using CadastreInvent.Inspection.Domain.Enums;

namespace CadastreInvent.Inspection.Application.Handlers
{
    public class GetMyMobileTasksQueryHandler : IRequestHandler<GetMyMobileTasksQuery, List<MobileTaskDto>>
    {
        private readonly CadastreDbContext _dbContext;

        public GetMyMobileTasksQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<MobileTaskDto>> Handle(GetMyMobileTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _dbContext.InspectionTasks
                .AsNoTracking()
                .Where(t => t.AssignedInspectorId == request.InspectorId && t.State != TaskState.RequiresRework && t.State != TaskState.Completed && t.State != TaskState.Verified)
                .OrderBy(t => t.Deadline)
                .ToListAsync(cancellationToken);

            if (!tasks.Any())
            {
                return new List<MobileTaskDto>();
            }

            var spatialUnitIds = tasks.Select(t => t.TargetSpatialUnitId).Distinct().ToList();

            var spatialUnits = await _dbContext.SpatialUnits
                .AsNoTracking()
                .Where(s => spatialUnitIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.ReferenceNumber, cancellationToken);

            var suIdsStr = string.Join("','", spatialUnitIds);
            var sql = $@"
                SELECT su.""Id"" as SpatialUnitId, pc.""CharacteristicsJson"" 
                FROM registry.spatial_units su
                JOIN registry.ba_unit_spatial_units bsu ON su.""Id"" = bsu.""SpatialUnitId""
                JOIN valuation.valuation_units vu ON bsu.""BAUnitId"" = vu.""BAUnitId""
                JOIN valuation.property_characteristics pc ON vu.""Id"" = pc.""ValuationUnitId""
                WHERE su.""Id"" IN ('{string.Join("','", spatialUnitIds)}') AND pc.""IsDeleted"" = false
            ";

            var characteristicsMap = new Dictionary<System.Guid, string>();

            try
            {
                using var command = _dbContext.Database.GetDbConnection().CreateCommand();
                command.CommandText = sql;
                await _dbContext.Database.OpenConnectionAsync(cancellationToken);
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    characteristicsMap[reader.GetGuid(0)] = reader.GetString(1);
                }
                await _dbContext.Database.CloseConnectionAsync();
            }
            catch
            {
            }

            return tasks.Select(t => new MobileTaskDto
            {
                Id = t.Id,
                SpatialUnitReference = spatialUnits.GetValueOrDefault(t.TargetSpatialUnitId, "Идентификатор утерян"),
                State = t.State,
                Deadline = t.Deadline,
                Longitude = t.TargetCoordinates.X,
                Latitude = t.TargetCoordinates.Y,
                RejectionReason = t.RejectionReason,
                CurrentCharacteristicsJson = characteristicsMap.GetValueOrDefault(t.TargetSpatialUnitId)
            }).ToList();
        }
    }
}