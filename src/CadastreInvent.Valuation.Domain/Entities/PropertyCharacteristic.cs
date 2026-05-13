using System;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Valuation.Domain.Entities
{
    public class PropertyCharacteristic : DomainEntity
    {
        public Guid ValuationUnitId { get; private set; }
        public string CharacteristicsJson { get; private set; }
        public bool HasViolations { get; private set; }

        protected PropertyCharacteristic() { }

        public PropertyCharacteristic(Guid valuationUnitId, string characteristicsJson)
        {
            if (valuationUnitId == Guid.Empty) throw new ArgumentException(nameof(valuationUnitId));
            if (string.IsNullOrWhiteSpace(characteristicsJson)) throw new ArgumentNullException(nameof(characteristicsJson));

            Id = Guid.NewGuid();
            ValuationUnitId = valuationUnitId;
            CharacteristicsJson = characteristicsJson;
            HasViolations = false;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateCharacteristics(string newCharacteristicsJson, bool? hasViolations = null)
        {
            if (string.IsNullOrWhiteSpace(newCharacteristicsJson)) throw new ArgumentNullException(nameof(newCharacteristicsJson));

            CharacteristicsJson = newCharacteristicsJson;

            if (hasViolations.HasValue)
            {
                HasViolations = hasViolations.Value;
            }

            UpdateTimestamp();
        }

        public void SetViolationStatus(bool hasViolations)
        {
            HasViolations = hasViolations;
            UpdateTimestamp();
        }
    }
}