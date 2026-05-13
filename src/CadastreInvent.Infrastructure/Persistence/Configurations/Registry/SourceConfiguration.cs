using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Registry
{
    public class SourceConfiguration : IEntityTypeConfiguration<Source>
    {
        public void Configure(EntityTypeBuilder<Source> builder)
        {
            builder.ToTable("sources", "registry");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).HasConversion<string>().IsRequired();
            builder.Property(x => x.DocumentNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.RecordDate).IsRequired();
            builder.Property(x => x.ContentUrl).HasMaxLength(1000);
        }
    }
}
