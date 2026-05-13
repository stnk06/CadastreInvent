using System;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.DTOs
{
    public class RRRDto
    {
        public Guid Id { get; set; }
        public RRRType Type { get; set; }
        public Guid BAUnitId { get; set; }
        public Guid? PartyId { get; set; }
        public Guid? PartyGroupId { get; set; }
        public Guid SourceId { get; set; }
        public decimal ShareNumerator { get; set; }
        public decimal ShareDenominator { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}