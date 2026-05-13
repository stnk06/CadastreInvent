using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Shared
{
    public class EventStreamConfiguration : IEntityTypeConfiguration<EventStream>
    {
        public void Configure(EntityTypeBuilder<EventStream> builder)
        {
            builder.ToTable("event_streams", "shared");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AggregateId).IsRequired();
            builder.Property(x => x.AggregateType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.EventDataJson).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.Timestamp).IsRequired();

            builder.HasIndex(x => new { x.AggregateId, x.Version }).IsUnique();
        }
    }
}
