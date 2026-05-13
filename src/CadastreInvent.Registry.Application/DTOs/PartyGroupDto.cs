using System;
using System.Collections.Generic;

namespace CadastreInvent.Registry.Application.DTOs
{
    public class PartyGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<PartyGroupMemberDto> Members { get; set; } = new();
    }
}