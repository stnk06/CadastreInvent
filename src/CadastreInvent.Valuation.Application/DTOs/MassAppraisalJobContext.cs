using System;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class MassAppraisalJobContext
    {
        public Guid JobId { get; set; }
        public Guid ModelId { get; set; }
        public string? ZoningStatusFilter { get; set; }
        public float? MinArea { get; set; }
        public float? MaxArea { get; set; }
    }
}