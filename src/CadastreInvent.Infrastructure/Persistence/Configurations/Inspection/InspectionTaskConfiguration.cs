using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Inspection.Domain.Entities;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Inspection.Domain.Enums; 

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Inspection
{
    public class InspectionTaskConfiguration : IEntityTypeConfiguration<InspectionTask>
    {
        public void Configure(EntityTypeBuilder<InspectionTask> builder)
        {
            builder.ToTable("inspection_tasks", "inspection");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TargetSpatialUnitId).IsRequired();
            builder.Property(x => x.AssignedInspectorId);
            builder.Property(x => x.State).HasConversion<string>().IsRequired();
            builder.Property(x => x.Deadline).IsRequired();
            builder.Property(x => x.Description).HasColumnType("text").IsRequired();
            builder.Property(x => x.RejectionReason).HasMaxLength(1000);
            builder.Property(x => x.TargetCoordinates).HasColumnType("geometry(Point, 4326)").IsRequired();

            builder.Property(x => x.ViolationStatus).HasConversion<string>().IsRequired().HasDefaultValue(ViolationStatus.None);

            builder.Property(x => x.Conclusion).HasColumnType("text");
            builder.Property(x => x.RecordedCoordinates).HasColumnType("geometry(Point, 4326)");
            builder.Property(x => x.HasGpsDiscrepancy).IsRequired().HasDefaultValue(false);

            builder.Property<uint>("xmin")
                   .HasColumnType("xid")
                   .ValueGeneratedOnAddOrUpdate()
                   .IsConcurrencyToken();

            builder.HasIndex(x => x.TargetCoordinates).HasMethod("gist");
            builder.HasIndex(x => x.RecordedCoordinates).HasMethod("gist");
            builder.HasIndex(x => x.TargetSpatialUnitId);
            builder.HasIndex(x => x.AssignedInspectorId);

            builder.HasOne<SpatialUnit>()
                   .WithMany()
                   .HasForeignKey(x => x.TargetSpatialUnitId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.AssignedInspectorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}