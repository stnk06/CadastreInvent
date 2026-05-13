using System;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.DTOs
{
    public class PartyDto
    {
        public Guid Id { get; set; }
        public string ExtId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public PartyType Type { get; set; }
        public string? ContactInfo { get; set; }
    }
}