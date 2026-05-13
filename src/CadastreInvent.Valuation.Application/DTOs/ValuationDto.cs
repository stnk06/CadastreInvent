using System;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class ValuationDto
    {
        public Guid Id { get; set; }
        public Guid ValuationUnitId { get; set; }
        public Guid? ModelId { get; set; }
        public decimal AssessedValue { get; set; }
        public DateTime ValuationDate { get; set; }
        public ValuationMethod Method { get; set; }
    }
}