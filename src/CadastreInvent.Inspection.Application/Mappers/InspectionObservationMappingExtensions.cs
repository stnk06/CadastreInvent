using CadastreInvent.Inspection.Application.DTOs;
using CadastreInvent.Inspection.Domain.Entities;

namespace CadastreInvent.Inspection.Application.Mappers
{
    public static class InspectionTaskMappingExtensions
    {
        public static DispatcherTaskDto ToDto(this InspectionTask task)
        {
            if (task == null) return null;

            return new DispatcherTaskDto
            {
                Id = task.Id,
                Status = task.State.ToString(),
                Deadline = task.Deadline,
                SpatialUnitId = task.TargetSpatialUnitId,
                InspectorId = task.AssignedInspectorId ?? System.Guid.Empty,
                TargetLat = task.TargetCoordinates?.Y ?? 0,
                TargetLon = task.TargetCoordinates?.X ?? 0
            };
        }
    }
}