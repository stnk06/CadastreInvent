using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Registry
{
    public class BatchRegistrationJobConfiguration : IEntityTypeConfiguration<BatchRegistrationJob>
    {
        public void Configure(EntityTypeBuilder<BatchRegistrationJob> builder)
        {
            builder.ToTable("batch_registration_jobs", "registry");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasConversion<string>().IsRequired();

            builder.HasMany(x => x.Items)
                   .WithOne()
                   .HasForeignKey(i => i.JobId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(BatchRegistrationJob.Items))!
                   .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}