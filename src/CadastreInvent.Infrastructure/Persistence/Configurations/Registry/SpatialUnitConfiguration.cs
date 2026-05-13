using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Registry
{
    public class SpatialUnitConfiguration : IEntityTypeConfiguration<SpatialUnit>
    {
        public void Configure(EntityTypeBuilder<SpatialUnit> builder)
        {
            builder.ToTable("spatial_units", "registry");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReferenceNumber)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.ReferenceNumber)
                .IsUnique();

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .IsRequired();

            builder.Property<DateTime>("ValidFrom")
                .IsRequired();

            builder.Property<DateTime>("ValidTo")
                .IsRequired();

            builder.Property(x => x.AreaSqMeters)
                .IsRequired();

            builder.Property(x => x.Boundary)
                .HasColumnType("geometry(Polygon, 4326)")
                .IsRequired();

            builder.HasIndex(x => x.Boundary)
                .HasMethod("gist");
        }
    }
}