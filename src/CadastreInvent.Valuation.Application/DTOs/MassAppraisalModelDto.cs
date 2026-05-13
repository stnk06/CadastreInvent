using System;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class MassAppraisalModelDto
    {
        public Guid Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Algorithm { get; set; } = string.Empty;
        public DateTime TrainingDate { get; set; }
    }
}