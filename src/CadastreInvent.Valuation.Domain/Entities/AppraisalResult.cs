using System;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Valuation.Domain.Entities
{
    public class AppraisalResult : DomainEntity
    {
        public Guid SpatialUnitId { get; private set; }
        public decimal CalculatedValue { get; private set; }
        public float ConfidenceScore { get; private set; }
        public DateTime AppraisalDate { get; private set; }
        public string MlModelVersion { get; private set; } = string.Empty;

        protected AppraisalResult() { }

        public AppraisalResult(Guid spatialUnitId, decimal calculatedValue, float confidenceScore, string mlModelVersion)
        {
            if (spatialUnitId == Guid.Empty) throw new ArgumentException(nameof(spatialUnitId));

            Id = Guid.NewGuid();
            SpatialUnitId = spatialUnitId;
            CalculatedValue = calculatedValue;
            ConfidenceScore = confidenceScore;
            MlModelVersion = mlModelVersion;
            AppraisalDate = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }
    }
}