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
    public record GetInspectionTaskByIdQuery(Guid Id) : IRequest<DispatcherTaskDto?>;

    public class GetInspectionTaskByIdQueryHandler : IRequestHandler<GetInspectionTaskByIdQuery, DispatcherTaskDto?>
    {
        private readonly CadastreDbContext _dbContext;

        public GetInspectionTaskByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DispatcherTaskDto?> Handle(GetInspectionTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var rawTask = await _dbContext.InspectionTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (rawTask == null) return null;

            var taskDto = new DispatcherTaskDto
            {
                Id = rawTask.Id,
                SpatialUnitId = rawTask.TargetSpatialUnitId,
                TargetLat = rawTask.TargetCoordinates.Y,
                TargetLon = rawTask.TargetCoordinates.X,
                RecordedLat = rawTask.RecordedCoordinates?.Y,
                RecordedLon = rawTask.RecordedCoordinates?.X,
                Description = rawTask.Description,
                Deadline = rawTask.Deadline,
                Status = rawTask.State.ToString(),
                RejectionReason = rawTask.RejectionReason,
                InspectorId = rawTask.AssignedInspectorId ?? Guid.Empty
            };

            var sql = $@"
                SELECT pc.""CharacteristicsJson"" 
                FROM registry.spatial_units su
                JOIN registry.ba_unit_spatial_units bsu ON su.""Id"" = bsu.""SpatialUnitId""
                JOIN valuation.valuation_units vu ON bsu.""BAUnitId"" = vu.""BAUnitId""
                JOIN valuation.property_characteristics pc ON vu.""Id"" = pc.""ValuationUnitId""
                WHERE su.""Id"" = '{taskDto.SpatialUnitId}' AND pc.""IsDeleted"" = false
                LIMIT 1
            ";

            using var command = _dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            bool wasClosed = command.Connection.State == ConnectionState.Closed;
            if (wasClosed) await command.Connection.OpenAsync(cancellationToken);
            try
            {
                var result = await command.ExecuteScalarAsync(cancellationToken);
                if (result != null && result != DBNull.Value)
                {
                    taskDto.CurrentCharacteristicsJson = result.ToString();
                }
            }
            catch { }
            finally { if (wasClosed) await command.Connection.CloseAsync(); }

            var photos = await _dbContext.InspectionPhotos
                .AsNoTracking()
                .Where(p => p.InspectionTaskId == taskDto.Id)
                .ToListAsync(cancellationToken);

            taskDto.Photos = photos.Select(p => new PhotoDto
            {
                Id = p.Id,
                PhotoUrl = p.FilePath,
                Lat = taskDto.RecordedLat ?? taskDto.TargetLat,
                Lon = taskDto.RecordedLon ?? taskDto.TargetLon,
                CaptureDate = p.CreatedAt
            }).ToList();

            var observations = await _dbContext.InspectionObservations
                .AsNoTracking()
                .Where(o => o.InspectionTaskId == taskDto.Id)
                .ToListAsync(cancellationToken);

            taskDto.Observations = observations.Select(o => new ObservationDto
            {
                Id = o.Id,
                Category = o.Category.ToString(),
                RemarksJson = o.RemarksJson,
                ObservationDate = o.ObservationDate
            }).ToList();

            return taskDto;
        }
    }
}