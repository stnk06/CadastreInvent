using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Mappers
{
    public static class ValuationUnitMappingExtensions
    {
        public static ValuationUnitDto ToDto(this ValuationUnit entity)
        {
            if (entity == null) return null;

            return new ValuationUnitDto
            {
                Id = entity.Id,
                BAUnitId = entity.BAUnitId,
                ZoningStatus = entity.ZoningStatus
            };
        }
    }
}