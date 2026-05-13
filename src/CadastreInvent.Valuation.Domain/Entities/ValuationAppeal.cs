using System;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Domain.Entities
{
    public class ValuationAppeal : DomainEntity
    {
        public Guid ValuationId { get; private set; }
        public Guid ApplicantPartyId { get; private set; }
        public AppealStatus Status { get; private set; }
        public string Reason { get; private set; }
        public DateTime SubmissionDate { get; private set; }

        protected ValuationAppeal() { }

        public ValuationAppeal(Guid valuationId, Guid applicantPartyId, string reason, DateTime submissionDate)
        {
            if (valuationId == Guid.Empty) throw new ArgumentException(nameof(valuationId));
            if (applicantPartyId == Guid.Empty) throw new ArgumentException(nameof(applicantPartyId));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentNullException(nameof(reason));

            Id = Guid.NewGuid();
            ValuationId = valuationId;
            ApplicantPartyId = applicantPartyId;
            Reason = reason;
            SubmissionDate = submissionDate;
            Status = AppealStatus.Submitted;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(AppealStatus newStatus)
        {
            Status = newStatus;
            UpdateTimestamp();
        }
    }
}