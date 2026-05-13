using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Registry
{
    public class BatchRegistrationItemConfiguration : IEntityTypeConfiguration<BatchRegistrationItem>
    {
        public void Configure(EntityTypeBuilder<BatchRegistrationItem> builder)
        {
            builder.ToTable("batch_registration_items", "registry");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.JobId).IsRequired();
            builder.Property(x => x.Wkt).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().IsRequired();
            builder.Property(x => x.ExtId).HasMaxLength(100);
            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

            builder.HasIndex(x => x.JobId);
            builder.HasIndex(x => x.Status);
        }
    }
}