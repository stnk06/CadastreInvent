using System;

namespace CadastreInvent.Shared.Domain.Entities
{
    public class EventStream
    {
        public Guid Id { get; private set; }
        public Guid AggregateId { get; private set; }
        public string AggregateType { get; private set; }
        public string EventType { get; private set; }
        public string EventDataJson { get; private set; }
        public int Version { get; private set; }
        public DateTime Timestamp { get; private set; }

        protected EventStream() { }

        public EventStream(Guid aggregateId, string aggregateType, string eventType, string eventDataJson, int version)
        {
            if (aggregateId == Guid.Empty) throw new ArgumentException(nameof(aggregateId));
            if (string.IsNullOrWhiteSpace(aggregateType)) throw new ArgumentNullException(nameof(aggregateType));
            if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentNullException(nameof(eventType));
            if (string.IsNullOrWhiteSpace(eventDataJson)) throw new ArgumentNullException(nameof(eventDataJson));
            if (version <= 0) throw new ArgumentException(nameof(version));

            Id = Guid.NewGuid();
            AggregateId = aggregateId;
            AggregateType = aggregateType;
            EventType = eventType;
            EventDataJson = eventDataJson;
            Version = version;
            Timestamp = DateTime.UtcNow;
        }
    }
}