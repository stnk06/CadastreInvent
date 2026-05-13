using System;
using NetTopologySuite.Geometries;
using CadastreInvent.Registry.Domain.Enums;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class SpatialUnit : DomainEntity
    {
        public string ReferenceNumber { get; private set; }
        public SpatialUnitType Type { get; private set; }
        public double AreaSqMeters { get; private set; }
        public Geometry Boundary { get; private set; }
        public int Srid { get; private set; }

        protected SpatialUnit() { }

        public SpatialUnit(string referenceNumber, SpatialUnitType type, Geometry boundary, double areaSqMeters, int srid = 4326)
        {
            Id = Guid.NewGuid();
            ReferenceNumber = referenceNumber;
            Type = type;
            if (boundary.SRID == 0)
            {
                boundary.SRID = srid;
            }
            Boundary = boundary;
            Srid = boundary.SRID;
            AreaSqMeters = areaSqMeters;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateBoundary(Geometry newBoundary, double areaSqMeters)
        {
            if (newBoundary.SRID == 0)
            {
                newBoundary.SRID = this.Srid;
            }
            Boundary = newBoundary;
            AreaSqMeters = areaSqMeters;
            UpdateTimestamp();
        }
    }
}