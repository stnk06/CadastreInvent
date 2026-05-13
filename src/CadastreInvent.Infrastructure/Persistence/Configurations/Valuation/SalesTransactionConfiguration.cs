using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Valuation.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Valuation
{
    public class SalesTransactionConfiguration : IEntityTypeConfiguration<SalesTransaction>
    {
        public void Configure(EntityTypeBuilder<SalesTransaction> builder)
        {
            builder.ToTable("sales_transactions", "valuation");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ValuationUnitId).IsRequired();
            builder.Property(x => x.SalePrice).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.TransactionDate).IsRequired();
            builder.Property(x => x.Validity).HasConversion<string>().IsRequired();

            builder.HasIndex(x => x.ValuationUnitId);

            builder.HasOne<ValuationUnit>()
                   .WithMany()
                   .HasForeignKey(x => x.ValuationUnitId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}