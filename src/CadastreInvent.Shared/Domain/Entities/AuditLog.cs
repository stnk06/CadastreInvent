using System;

namespace CadastreInvent.Shared.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; private set; }
        public Guid EntityId { get; private set; }
        public string EntityName { get; private set; }
        public string Action { get; private set; }
        public string ChangesJson { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime Timestamp { get; private set; }

        protected AuditLog() { }

        public AuditLog(Guid entityId, string entityName, string action, string changesJson, Guid userId)
        {
            Id = Guid.NewGuid();
            EntityId = entityId;
            EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            ChangesJson = changesJson ?? throw new ArgumentNullException(nameof(changesJson));
            UserId = userId;
            Timestamp = DateTime.UtcNow;
        }
    }
}