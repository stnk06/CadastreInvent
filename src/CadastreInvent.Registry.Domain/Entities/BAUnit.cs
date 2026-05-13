using System;
using System.Collections.Generic;
using System.Linq;
using CadastreInvent.Registry.Domain.Enums;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class BAUnit : DomainEntity
    {
        public string Name { get; private set; }
        public BAUnitType Type { get; private set; }

        private readonly List<BAUnitSpatialUnit> _spatialUnits = new();
        public IReadOnlyCollection<BAUnitSpatialUnit> SpatialUnits => _spatialUnits.AsReadOnly();

        private readonly List<RRR> _rrrs = new();
        public IReadOnlyCollection<RRR> Rrrs => _rrrs.AsReadOnly();

        protected BAUnit() { }

        public BAUnit(string name, BAUnitType type)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Id = Guid.NewGuid();
            Name = name;
            Type = type;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddSpatialUnit(Guid spatialUnitId)
        {
            if (_spatialUnits.Any(su => su.SpatialUnitId == spatialUnitId)) throw new InvalidOperationException($"Пространственный контур с ID {spatialUnitId} уже привязан к объекту.");
            _spatialUnits.Add(new BAUnitSpatialUnit(Id, spatialUnitId));
            UpdateTimestamp();
        }

        public void RemoveSpatialUnit(Guid spatialUnitId)
        {
            var link = _spatialUnits.FirstOrDefault(su => su.SpatialUnitId == spatialUnitId);
            if (link == null) throw new InvalidOperationException($"Пространственный контур с ID {spatialUnitId} не привязан к данному объекту.");
            _spatialUnits.Remove(link);
            UpdateTimestamp();
        }

        public void AddRRR(RRR rrr)
        {
            if (rrr == null) throw new ArgumentNullException(nameof(rrr));
            _rrrs.Add(rrr);
            UpdateTimestamp();
        }

        public void TerminateRRR(Guid rrrId, DateTime endDate)
        {
            var rrr = _rrrs.FirstOrDefault(r => r.Id == rrrId);
            if (rrr == null) throw new InvalidOperationException($"Право (RRR) с ID {rrrId} не найдено в данном объекте.");
            rrr.Terminate(endDate);
            UpdateTimestamp();
        }
    }
}