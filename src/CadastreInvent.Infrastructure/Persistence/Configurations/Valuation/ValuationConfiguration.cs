using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Valuation.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Valuation
{
    public class ValuationConfiguration : IEntityTypeConfiguration<CadastreInvent.Valuation.Domain.Entities.Valuation>
    {
        public void Configure(EntityTypeBuilder<CadastreInvent.Valuation.Domain.Entities.Valuation> builder)
        {
            builder.ToTable("valuations", "valuation");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ValuationUnitId).IsRequired();
            builder.Property(x => x.ModelId);
            builder.Property(x => x.AssessedValue).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.ValuationDate).IsRequired();
            builder.Property(x => x.Method).HasConversion<string>().IsRequired();

            builder.HasIndex(x => x.ValuationUnitId);

            builder.HasOne<ValuationUnit>()
                   .WithMany()
                   .HasForeignKey(x => x.ValuationUnitId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<MassAppraisalModel>()
                   .WithMany()
                   .HasForeignKey(x => x.ModelId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}