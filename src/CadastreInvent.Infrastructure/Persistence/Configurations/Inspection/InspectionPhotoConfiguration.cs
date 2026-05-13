using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Inspection.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Inspection
{
    public class InspectionPhotoConfiguration : IEntityTypeConfiguration<InspectionPhoto>
    {
        public void Configure(EntityTypeBuilder<InspectionPhoto> builder)
        {
            builder.ToTable("inspection_photos", "inspection");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.InspectionTaskId).IsRequired();

            builder.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();

            builder.Property(x => x.AppLocalId);

            builder.HasIndex(x => x.InspectionTaskId);

            builder.HasIndex(x => x.AppLocalId).IsUnique().HasFilter("\"AppLocalId\" IS NOT NULL");

            builder.HasOne<InspectionTask>()
                   .WithMany()
                   .HasForeignKey(x => x.InspectionTaskId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}