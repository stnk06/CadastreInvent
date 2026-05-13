using System;

namespace CadastreInvent.Registry.Application.DTOs
{
    public class PartyGroupMemberDto
    {
        public Guid PartyId { get; set; }
        public decimal ShareNumerator { get; set; }
        public decimal ShareDenominator { get; set; }
    }
}