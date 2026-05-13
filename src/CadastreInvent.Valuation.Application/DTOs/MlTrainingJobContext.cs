using System;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class MlTrainingJobContext
    {
        public Guid JobId { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}