using System.Linq;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.DTOs;

namespace CadastreInvent.Registry.Application.Mappers
{
    public static class BAUnitMappingExtensions
    {
        public static BAUnitDto ToDto(this BAUnit entity)
        {
            if (entity == null) return null;

            return new BAUnitDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                SpatialUnitIds = entity.SpatialUnits?.Select(su => su.SpatialUnitId).ToList() ?? new(),
                Rrrs = entity.Rrrs?.Select(r => r.ToDto()).ToList() ?? new()
            };
        }
    }
}