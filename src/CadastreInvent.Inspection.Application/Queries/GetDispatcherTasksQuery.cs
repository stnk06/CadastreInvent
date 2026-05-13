using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Inspection.Application.DTOs;

namespace CadastreInvent.Inspection.Application.Queries
{
    public record GetDispatcherTasksQuery() : IRequest<List<DispatcherTaskDto>>;

    public class GetDispatcherTasksQueryHandler : IRequestHandler<GetDispatcherTasksQuery, List<DispatcherTaskDto>>
    {
        private readonly CadastreDbContext _dbContext;

        public GetDispatcherTasksQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DispatcherTaskDto>> Handle(GetDispatcherTasksQuery request, CancellationToken cancellationToken)
        {
            var rawTasks = await _dbContext.InspectionTasks
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);

            if (!rawTasks.Any()) return new List<DispatcherTaskDto>();

            var suIds = rawTasks.Select(t => t.TargetSpatialUnitId).Distinct().ToList();

            var spatialUnits = await _dbContext.SpatialUnits
                .AsNoTracking()
                .Where(su => suIds.Contains(su.Id))
                .ToDictionaryAsync(su => su.Id, cancellationToken);

            var characteristicsMap = new Dictionary<Guid, string>();
            if (suIds.Any())
            {
                var sql = $@"
                    SELECT su.""Id"", pc.""CharacteristicsJson"" 
                    FROM registry.spatial_units su
                    JOIN registry.ba_unit_spatial_units bsu ON su.""Id"" = bsu.""SpatialUnitId""
                    JOIN valuation.valuation_units vu ON bsu.""BAUnitId"" = vu.""BAUnitId""
                    JOIN valuation.property_characteristics pc ON vu.""Id"" = pc.""ValuationUnitId""
                    WHERE su.""Id"" IN ('{string.Join("','", suIds)}') AND pc.""IsDeleted"" = false
                ";

                using var command = _dbContext.Database.GetDbConnection().CreateCommand();
                command.CommandText = sql;
                bool wasClosed = command.Connection.State == ConnectionState.Closed;
                if (wasClosed) await command.Connection.OpenAsync(cancellationToken);
                try
                {
                    using var reader = await command.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        characteristicsMap[reader.GetGuid(0)] = reader.GetString(1);
                    }
                }
                catch { }
                finally { if (wasClosed) await command.Connection.CloseAsync(); }
            }

            var userIds = rawTasks.Where(t => t.AssignedInspectorId.HasValue).Select(t => t.AssignedInspectorId!.Value).Distinct().ToList();
            var users = await _dbContext.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Username, cancellationToken);

            var taskIds = rawTasks.Select(t => t.Id).ToList();

            var allPhotos = await _dbContext.InspectionPhotos
                .AsNoTracking()
                .Where(p => taskIds.Contains(p.InspectionTaskId))
                .ToListAsync(cancellationToken);

            var allObservations = await _dbContext.InspectionObservations
                .AsNoTracking()
                .Where(o => taskIds.Contains(o.InspectionTaskId))
                .ToListAsync(cancellationToken);

            var result = new List<DispatcherTaskDto>();

            foreach (var t in rawTasks)
            {
                var dto = new DispatcherTaskDto
                {
                    Id = t.Id,
                    SpatialUnitId = t.TargetSpatialUnitId,
                    TargetLat = t.TargetCoordinates.Y,
                    TargetLon = t.TargetCoordinates.X,
                    RecordedLat = t.RecordedCoordinates?.Y,
                    RecordedLon = t.RecordedCoordinates?.X,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    Status = t.State.ToString(),
                    RejectionReason = t.RejectionReason,
                    InspectorId = t.AssignedInspectorId ?? Guid.Empty
                };

                if (spatialUnits.TryGetValue(t.TargetSpatialUnitId, out var su))
                {
                    dto.SpatialUnitReference = su.ReferenceNumber ?? su.Id.ToString();
                    if (su.Boundary != null) dto.PolygonWkt = su.Boundary.AsText();
                }

                if (t.AssignedInspectorId.HasValue && users.TryGetValue(t.AssignedInspectorId.Value, out var userName))
                {
                    dto.InspectorName = userName;
                }
                else
                {
                    dto.InspectorName = "Не назначен";
                }

                dto.CurrentCharacteristicsJson = characteristicsMap.GetValueOrDefault(t.TargetSpatialUnitId);

                dto.Photos = allPhotos.Where(p => p.InspectionTaskId == t.Id).Select(p => new PhotoDto
                {
                    Id = p.Id,
                    PhotoUrl = p.FilePath,
                    Lat = t.RecordedCoordinates?.Y ?? t.TargetCoordinates.Y,
                    Lon = t.RecordedCoordinates?.X ?? t.TargetCoordinates.X,
                    CaptureDate = p.CreatedAt
                }).ToList();

                dto.Observations = allObservations.Where(o => o.InspectionTaskId == t.Id).Select(o => new ObservationDto
                {
                    Id = o.Id,
                    Category = o.Category.ToString(),
                    RemarksJson = o.RemarksJson,
                    ObservationDate = o.ObservationDate
                }).ToList();

                result.Add(dto);
            }

            return result;
        }
    }
}