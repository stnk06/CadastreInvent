using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Mappers
{
    public static class PropertyCharacteristicMappingExtensions
    {
        public static PropertyCharacteristicDto ToDto(this PropertyCharacteristic entity)
        {
            if (entity == null) return null;

            return new PropertyCharacteristicDto
            {
                Id = entity.Id,
                ValuationUnitId = entity.ValuationUnitId,
                CharacteristicsJson = entity.CharacteristicsJson
            };
        }
    }
}