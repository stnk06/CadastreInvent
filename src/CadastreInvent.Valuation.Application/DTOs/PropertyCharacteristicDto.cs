using System;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class PropertyCharacteristicDto
    {
        public Guid Id { get; set; }
        public Guid ValuationUnitId { get; set; }
        public string CharacteristicsJson { get; set; } = string.Empty;
    }
}