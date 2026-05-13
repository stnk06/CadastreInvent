using System;
using System.Collections.Generic;

namespace CadastreInvent.Inspection.Application.DTOs
{
    public class DispatcherTaskDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public Guid SpatialUnitId { get; set; }
        public string SpatialUnitReference { get; set; } = string.Empty;
        public string PolygonWkt { get; set; } = string.Empty;
        public Guid InspectorId { get; set; }
        public string InspectorName { get; set; } = string.Empty;
        public double TargetLat { get; set; }
        public double TargetLon { get; set; }
        public double? RecordedLat { get; set; }
        public double? RecordedLon { get; set; }
        public string? RejectionReason { get; set; }
        public string Description { get; set; } = string.Empty;

        public string? CurrentCharacteristicsJson { get; set; }

        public List<ObservationDto> Observations { get; set; } = new();
        public List<PhotoDto> Photos { get; set; } = new();
    }

    public class ObservationDto
    {
        public Guid Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string RemarksJson { get; set; } = string.Empty;
        public DateTime ObservationDate { get; set; }
    }

    public class PhotoDto
    {
        public Guid Id { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lon { get; set; }
        public DateTime CaptureDate { get; set; }
    }
}