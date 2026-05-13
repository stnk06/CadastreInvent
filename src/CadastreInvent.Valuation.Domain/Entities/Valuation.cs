using System;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Domain.Entities
{
    public class Valuation : DomainEntity
    {
        public Guid ValuationUnitId { get; private set; }
        public Guid? ModelId { get; private set; }
        public decimal AssessedValue { get; private set; }
        public DateTime ValuationDate { get; private set; }
        public ValuationMethod Method { get; private set; }

        protected Valuation() { }

        public Valuation(Guid valuationUnitId, decimal assessedValue, DateTime valuationDate, ValuationMethod method, Guid? modelId = null)
        {
            if (valuationUnitId == Guid.Empty) throw new ArgumentException(nameof(valuationUnitId));
            if (assessedValue < 0) throw new ArgumentException(nameof(assessedValue));
            if (method == ValuationMethod.AutomatedMachineLearning && modelId == null) throw new ArgumentException(nameof(modelId));

            Id = Guid.NewGuid();
            ValuationUnitId = valuationUnitId;
            AssessedValue = assessedValue;
            ValuationDate = valuationDate;
            Method = method;
            ModelId = modelId;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateMassAppraisal(decimal newAssessedValue, Guid modelId)
        {
            if (newAssessedValue < 0) throw new ArgumentException(nameof(newAssessedValue));
            AssessedValue = newAssessedValue;
            ModelId = modelId;
            Method = ValuationMethod.AutomatedMachineLearning;
            ValuationDate = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void ApplyAppealDecision(decimal newAssessedValue)
        {
            if (newAssessedValue < 0) throw new ArgumentException(nameof(newAssessedValue));

            AssessedValue = newAssessedValue;
            Method = ValuationMethod.Comparative;
            ModelId = null;
            ValuationDate = DateTime.UtcNow;
            UpdateTimestamp();
        }
    }
}