using System;

namespace CadastreInvent.Shared.Application.Admin.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Changes { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}