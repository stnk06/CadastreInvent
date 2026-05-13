using System;

namespace CadastreInvent.Shared.Domain.Entities
{
    public class RolePermission : DomainEntity
    {
        public Guid RoleId { get; private set; }
        public string Permission { get; private set; }

        protected RolePermission() { }

        public RolePermission(Guid roleId, string permission)
        {
            Id = Guid.NewGuid();
            RoleId = roleId;
            Permission = permission;
            CreatedAt = DateTime.UtcNow;
        }
    }
}