using System;

namespace CadastreInvent.Registry.Domain.Entities
{
    public abstract class DomainEntity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        // Флаги мягкого удаления (Soft Delete)
        public bool IsDeleted { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }

        protected void UpdateTimestamp() => UpdatedAt = DateTime.UtcNow;

        public void MarkAsDeleted()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        // Системные колонки для версионирования (Temporal Tables)
        public DateTime ValidFrom { get; private set; }
        public DateTime ValidTo { get; private set; }
    }
}