using System;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class RRR : DomainEntity
    {
        public RRRType Type { get; private set; }
        public Guid BAUnitId { get; private set; }
        public Guid? PartyId { get; private set; }
        public Guid? PartyGroupId { get; private set; }
        public Guid SourceId { get; private set; }
        public decimal ShareNumerator { get; private set; }
        public decimal ShareDenominator { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        protected RRR() { }

        public RRR(RRRType type, Guid baUnitId, Guid sourceId, decimal numerator, decimal denominator, DateTime startDate, Guid? partyId = null, Guid? partyGroupId = null)
        {
            if (partyId == null && partyGroupId == null) throw new ArgumentException();
            if (partyId != null && partyGroupId != null) throw new ArgumentException();
            if (numerator <= 0 || denominator <= 0 || numerator > denominator) throw new ArgumentException();

            Id = Guid.NewGuid();
            Type = type;
            BAUnitId = baUnitId;
            SourceId = sourceId;
            ShareNumerator = numerator;
            ShareDenominator = denominator;
            StartDate = startDate;
            PartyId = partyId;
            PartyGroupId = partyGroupId;
            CreatedAt = DateTime.UtcNow;
        }

        public void Terminate(DateTime endDate)
        {
            if (endDate < StartDate) throw new ArgumentException();
            if (EndDate.HasValue) throw new InvalidOperationException();

            EndDate = endDate;
            UpdateTimestamp();
        }
    }
}