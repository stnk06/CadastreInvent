using System;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class PartyGroupMember
    {
        public Guid PartyGroupId { get; private set; }
        public Guid PartyId { get; private set; }
        public decimal ShareNumerator { get; private set; }
        public decimal ShareDenominator { get; private set; }

        protected PartyGroupMember() { }

        public PartyGroupMember(Guid partyGroupId, Guid partyId, decimal numerator, decimal denominator)
        {
            if (numerator <= 0 || denominator <= 0 || numerator > denominator)
                throw new ArgumentException();

            PartyGroupId = partyGroupId;
            PartyId = partyId;
            ShareNumerator = numerator;
            ShareDenominator = denominator;
        }
    }
}