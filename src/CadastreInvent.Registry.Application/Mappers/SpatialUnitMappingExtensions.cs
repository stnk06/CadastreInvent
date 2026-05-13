using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.DTOs;

namespace CadastreInvent.Registry.Application.Mappers
{
    public static class SpatialUnitMappingExtensions
    {
        public static SpatialUnitDto ToDto(this SpatialUnit entity)
        {
            if (entity == null) return null;

            return new SpatialUnitDto
            {
                Id = entity.Id,
                ReferenceNumber = entity.ReferenceNumber,
                Type = entity.Type,
                BoundaryWkt = entity.Boundary?.ToString() ?? string.Empty,
                AreaSqMeters = entity.AreaSqMeters
            };
        }
    }
}