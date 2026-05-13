using System;

namespace CadastreInvent.Registry.Application.DTOs
{
    public class BatchJobStatusDto
    {
        public Guid JobId { get; set; }
        public int TotalCount { get; set; }
        public int ProcessedCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}