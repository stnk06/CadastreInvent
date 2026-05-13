using System;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class ValuationUnitDto
    {
        public Guid Id { get; set; }
        public Guid BAUnitId { get; set; }
        public string ZoningStatus { get; set; } = string.Empty;
    }
}