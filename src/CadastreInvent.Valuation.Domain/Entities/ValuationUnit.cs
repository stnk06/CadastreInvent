using System;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Valuation.Domain.Entities
{
    public class ValuationUnit : DomainEntity
    {
        public Guid BAUnitId { get; private set; }
        public string ZoningStatus { get; private set; }

        protected ValuationUnit() { }

        public ValuationUnit(Guid baUnitId, string zoningStatus)
        {
            if (baUnitId == Guid.Empty) throw new ArgumentException(nameof(baUnitId));
            if (string.IsNullOrWhiteSpace(zoningStatus)) throw new ArgumentNullException(nameof(zoningStatus));

            Id = Guid.NewGuid();
            BAUnitId = baUnitId;
            ZoningStatus = zoningStatus;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateZoningStatus(string newZoningStatus)
        {
            if (string.IsNullOrWhiteSpace(newZoningStatus)) throw new ArgumentNullException(nameof(newZoningStatus));
            ZoningStatus = newZoningStatus;
            UpdateTimestamp();
        }
    }
}