using System;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class Party : DomainEntity
    {
        public string ExtId { get; private set; }
        public string Name { get; private set; }
        public PartyType Type { get; private set; }
        public string ContactInfo { get; private set; }

        protected Party() { }

        public Party(string extId, string name, PartyType type, string contactInfo)
        {
            if (string.IsNullOrWhiteSpace(extId)) throw new ArgumentNullException(nameof(extId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            Id = Guid.NewGuid();
            ExtId = extId;
            Name = name;
            Type = type;
            ContactInfo = contactInfo;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateContactInfo(string newContactInfo)
        {
            ContactInfo = newContactInfo ?? throw new ArgumentNullException(nameof(newContactInfo));
            UpdateTimestamp();
        }
    }
}