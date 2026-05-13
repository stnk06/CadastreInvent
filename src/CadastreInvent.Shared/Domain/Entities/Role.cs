using System;
using System.Collections.Generic;

namespace CadastreInvent.Shared.Domain.Entities
{
    public class Role : DomainEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        private readonly List<RolePermission> _permissions = new();
        public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

        protected Role() { }

        public Role(string name, string description)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }

        public void ClearPermissions()
        {
            _permissions.Clear();
            UpdateTimestamp();
        }

        public void AddPermission(string permission)
        {
            _permissions.Add(new RolePermission(this.Id, permission));
            UpdateTimestamp();
        }
    }
}