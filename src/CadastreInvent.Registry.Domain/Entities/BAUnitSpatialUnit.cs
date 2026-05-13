using System;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class BAUnitSpatialUnit
    {
        public Guid BAUnitId { get; private set; }
        public Guid SpatialUnitId { get; private set; }

        protected BAUnitSpatialUnit() { }

        public BAUnitSpatialUnit(Guid baUnitId, Guid spatialUnitId)
        {
            if (baUnitId == Guid.Empty) throw new ArgumentException(nameof(baUnitId));
            if (spatialUnitId == Guid.Empty) throw new ArgumentException(nameof(spatialUnitId));

            BAUnitId = baUnitId;
            SpatialUnitId = spatialUnitId;
        }
    }
}