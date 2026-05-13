using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Valuation.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Valuation
{
    public class PropertyCharacteristicConfiguration : IEntityTypeConfiguration<PropertyCharacteristic>
    {
        public void Configure(EntityTypeBuilder<PropertyCharacteristic> builder)
        {
            builder.ToTable("property_characteristics", "valuation");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ValuationUnitId)
                .IsRequired();

            builder.Property(x => x.CharacteristicsJson)
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(x => x.HasViolations)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(x => x.CharacteristicsJson)
                .HasMethod("gin");

            builder.HasOne<ValuationUnit>()
                .WithOne()
                .HasForeignKey<PropertyCharacteristic>(x => x.ValuationUnitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property<uint>("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }
    }
}