using System;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class AppraisalRequest
    {
        public Guid SpatialUnitId { get; set; }

        public AppraisalRequest(Guid spatialUnitId)
        {
            SpatialUnitId = spatialUnitId;
        }
    }
}