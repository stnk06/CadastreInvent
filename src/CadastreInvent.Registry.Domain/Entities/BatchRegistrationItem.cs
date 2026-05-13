using System;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class BatchRegistrationItem : DomainEntity
    {
        public Guid JobId { get; private set; }
        public string Wkt { get; private set; }
        public BatchItemStatus Status { get; private set; }
        public string? ExtId { get; private set; }
        public string? ErrorMessage { get; private set; }

        protected BatchRegistrationItem() { }

        public BatchRegistrationItem(Guid jobId, string wkt)
        {
            if (jobId == Guid.Empty) throw new ArgumentException(nameof(jobId));
            if (string.IsNullOrWhiteSpace(wkt)) throw new ArgumentNullException(nameof(wkt));

            Id = Guid.NewGuid();
            JobId = jobId;
            Wkt = wkt;
            Status = BatchItemStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkProcessed(string extId)
        {
            Status = BatchItemStatus.Processed;
            ExtId = extId ?? throw new ArgumentNullException(nameof(extId));
            UpdateTimestamp();
        }

        public void MarkFailed(string errorMessage)
        {
            Status = BatchItemStatus.Failed;
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            UpdateTimestamp();
        }
    }
}