using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Inspection.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Inspection
{
    public class InspectionObservationConfiguration : IEntityTypeConfiguration<InspectionObservation>
    {
        public void Configure(EntityTypeBuilder<InspectionObservation> builder)
        {
            builder.ToTable("inspection_observations", "inspection");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.InspectionTaskId).IsRequired();

            builder.Property(x => x.Category).HasConversion<string>().IsRequired();

            builder.Property(x => x.ObservationDate).IsRequired();

            builder.Property(x => x.RemarksJson).HasColumnType("jsonb").IsRequired();

            builder.Property(x => x.AppLocalId);

            builder.HasIndex(x => x.InspectionTaskId);

            builder.HasIndex(x => x.AppLocalId).IsUnique().HasFilter("\"AppLocalId\" IS NOT NULL");
        }
    }
}