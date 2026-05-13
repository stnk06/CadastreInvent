using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Inspection.Application.DTOs;

namespace CadastreInvent.Inspection.Application.Queries
{
    public record GetInspectionTasksQuery() : IRequest<List<DispatcherTaskDto>>;

    public class GetInspectionTasksQueryHandler : IRequestHandler<GetInspectionTasksQuery, List<DispatcherTaskDto>>
    {
        private readonly CadastreDbContext _dbContext;

        public GetInspectionTasksQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DispatcherTaskDto>> Handle(GetInspectionTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _dbContext.InspectionTasks
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new DispatcherTaskDto
                {
                    Id = t.Id,
                    SpatialUnitId = t.TargetSpatialUnitId,
                    TargetLat = t.TargetCoordinates.Y,
                    TargetLon = t.TargetCoordinates.X,
                    RecordedLat = t.RecordedCoordinates != null ? t.RecordedCoordinates.Y : null,
                    RecordedLon = t.RecordedCoordinates != null ? t.RecordedCoordinates.X : null,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    Status = t.State.ToString(),
                    RejectionReason = t.RejectionReason,
                    InspectorId = t.AssignedInspectorId ?? Guid.Empty
                })
                .ToListAsync(cancellationToken);

            return tasks;
        }
    }
}