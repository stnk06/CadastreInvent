using System;
using CadastreInvent.Inspection.Domain.Enums;

namespace CadastreInvent.Inspection.Application.DTOs
{
    public class InspectionObservationDto
    {
        public Guid Id { get; set; }
        public Guid FieldTaskId { get; set; }
        public ObservationCategory Category { get; set; }
        public string RemarksJson { get; set; } = string.Empty;
        public DateTime ObservationDate { get; set; }
    }
}