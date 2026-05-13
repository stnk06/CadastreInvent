using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Shared
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("audit_logs", "shared");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.EntityId).IsRequired();
            builder.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Action).HasConversion<string>().IsRequired();
            builder.Property(x => x.ChangesJson).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Timestamp).IsRequired();

            builder.HasIndex(x => x.EntityId);
        }
    }
}
