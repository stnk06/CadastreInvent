using System;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.DTOs
{
    public class SpatialUnitDto
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public SpatialUnitType Type { get; set; }
        public string BoundaryWkt { get; set; } = string.Empty;
        public double AreaSqMeters { get; set; }
    }
}