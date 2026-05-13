using System;
using CadastreInvent.Inspection.Domain.Enums;

namespace CadastreInvent.Inspection.Application.DTOs
{
    public class MobileTaskDto
    {
        public Guid Id { get; set; }
        public string SpatialUnitReference { get; set; } = string.Empty;
        public TaskState State { get; set; }
        public DateTime Deadline { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string? RejectionReason { get; set; }
        public string? CurrentCharacteristicsJson { get; set; }
    }
}