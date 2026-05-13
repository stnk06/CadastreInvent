using System;

namespace CadastreInvent.Inspection.Application.DTOs
{
    public class InspectionPhotoDto
    {
        public Guid Id { get; set; }
        public Guid FieldTaskId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Azimuth { get; set; }
        public DateTime CaptureDate { get; set; }
    }
}