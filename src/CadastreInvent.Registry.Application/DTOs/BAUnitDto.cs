using System;
using System.Collections.Generic;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.DTOs
{
    public class BAUnitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BAUnitType Type { get; set; }
        public List<Guid> SpatialUnitIds { get; set; } = new();
        public List<RRRDto> Rrrs { get; set; } = new();
    }
}