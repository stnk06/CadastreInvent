using System;
using System.Collections.Generic;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class BatchRegistrationJob : DomainEntity
    {
        public int TotalCount { get; private set; }
        public int ProcessedCount { get; private set; }
        public BatchJobStatus Status { get; private set; }

        private readonly List<BatchRegistrationItem> _items = new();
        public IReadOnlyCollection<BatchRegistrationItem> Items => _items.AsReadOnly();

        protected BatchRegistrationJob() { }

        public BatchRegistrationJob(int totalCount)
        {
            if (totalCount <= 0) throw new ArgumentException(nameof(totalCount));

            Id = Guid.NewGuid();
            TotalCount = totalCount;
            ProcessedCount = 0;
            Status = BatchJobStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddItem(string wkt)
        {
            if (string.IsNullOrWhiteSpace(wkt)) throw new ArgumentNullException(nameof(wkt));
            _items.Add(new BatchRegistrationItem(Id, wkt));
        }

        public void IncrementProcessed()
        {
            ProcessedCount++;
            if (Status == BatchJobStatus.Pending)
            {
                Status = BatchJobStatus.Processing;
            }
            UpdateTimestamp();
        }

        public void Complete()
        {
            Status = BatchJobStatus.Completed;
            UpdateTimestamp();
        }
    }
}