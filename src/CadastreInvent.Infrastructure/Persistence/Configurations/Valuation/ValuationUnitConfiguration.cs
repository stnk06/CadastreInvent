using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Valuation
{
    public class ValuationUnitConfiguration : IEntityTypeConfiguration<ValuationUnit>
    {
        public void Configure(EntityTypeBuilder<ValuationUnit> builder)
        {
            builder.ToTable("valuation_units", "valuation");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.BAUnitId).IsRequired();
            builder.HasIndex(x => x.BAUnitId).IsUnique();
            builder.Property(x => x.ZoningStatus).HasMaxLength(100).IsRequired();

            builder.HasOne<BAUnit>()
                   .WithOne()
                   .HasForeignKey<ValuationUnit>(x => x.BAUnitId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property<uint>("xmin")
                   .HasColumnType("xid")
                   .ValueGeneratedOnAddOrUpdate()
                   .IsConcurrencyToken();
        }
    }
}