using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Registry
{
    public class BAUnitConfiguration : IEntityTypeConfiguration<BAUnit>
    {
        public void Configure(EntityTypeBuilder<BAUnit> builder)
        {
            builder.ToTable("ba_units", "registry");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
            builder.Property(x => x.Type).HasConversion<string>().IsRequired();

            builder.Property<DateTime>("ValidFrom").IsRequired();
            builder.Property<DateTime>("ValidTo").IsRequired();

            builder.OwnsMany(x => x.SpatialUnits, su =>
            {
                su.ToTable("ba_unit_spatial_units", "registry");
                su.WithOwner().HasForeignKey(x => x.BAUnitId);
                su.HasKey(x => new { x.BAUnitId, x.SpatialUnitId });
                su.Property(x => x.BAUnitId).ValueGeneratedNever();
                su.Property(x => x.SpatialUnitId).ValueGeneratedNever();

                su.HasOne<SpatialUnit>()
                  .WithMany()
                  .HasForeignKey(x => x.SpatialUnitId)
                  .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Metadata.FindNavigation(nameof(BAUnit.SpatialUnits))!
                   .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Rrrs)
                   .WithOne()
                   .HasForeignKey(r => r.BAUnitId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(BAUnit.Rrrs))!
                   .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}