using System;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Inspection.Domain.Enums;

namespace CadastreInvent.Inspection.Domain.Entities
{
    public class InspectionObservation : DomainEntity
    {
        public Guid InspectionTaskId { get; private set; }
        public ObservationCategory Category { get; private set; }
        public string RemarksJson { get; private set; }
        public DateTime ObservationDate { get; private set; }
        public Guid? AppLocalId { get; private set; }

        protected InspectionObservation() { }

        public InspectionObservation(Guid inspectionTaskId, ObservationCategory category, string remarksJson, DateTime observationDate, Guid? appLocalId = null)
        {
            if (inspectionTaskId == Guid.Empty) throw new ArgumentException(nameof(inspectionTaskId));
            if (string.IsNullOrWhiteSpace(remarksJson)) throw new ArgumentNullException(nameof(remarksJson));

            Id = Guid.NewGuid();
            InspectionTaskId = inspectionTaskId;
            Category = category;
            RemarksJson = remarksJson;
            ObservationDate = observationDate;
            AppLocalId = appLocalId;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateRemarks(string newRemarksJson)
        {
            if (string.IsNullOrWhiteSpace(newRemarksJson)) throw new ArgumentNullException(nameof(newRemarksJson));

            RemarksJson = newRemarksJson;
            UpdateTimestamp();
        }
    }
}