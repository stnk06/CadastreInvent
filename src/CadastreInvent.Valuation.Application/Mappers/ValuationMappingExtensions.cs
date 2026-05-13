using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Mappers
{
    public static class ValuationMappingExtensions
    {
        public static ValuationDto ToDto(this CadastreInvent.Valuation.Domain.Entities.Valuation entity)
        {
            if (entity == null) return null;

            return new ValuationDto
            {
                Id = entity.Id,
                ValuationUnitId = entity.ValuationUnitId,
                ModelId = entity.ModelId,
                AssessedValue = entity.AssessedValue,
                ValuationDate = entity.ValuationDate,
                Method = entity.Method
            };
        }
    }
}