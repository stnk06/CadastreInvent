using System;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class Source : DomainEntity
    {
        public SourceType Type { get; private set; }
        public string DocumentNumber { get; private set; }
        public DateTime RecordDate { get; private set; }
        public string ContentUrl { get; private set; }

        protected Source() { }

        public Source(SourceType type, string documentNumber, DateTime recordDate, string contentUrl)
        {
            if (string.IsNullOrWhiteSpace(documentNumber)) throw new ArgumentNullException(nameof(documentNumber));

            Id = Guid.NewGuid();
            Type = type;
            DocumentNumber = documentNumber;
            RecordDate = recordDate;
            ContentUrl = contentUrl;
            CreatedAt = DateTime.UtcNow;
        }
    }
}