using System;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class ValuationHistoryDto
    {
        public decimal AssessedValue { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsCurrent { get; set; }
    }
}