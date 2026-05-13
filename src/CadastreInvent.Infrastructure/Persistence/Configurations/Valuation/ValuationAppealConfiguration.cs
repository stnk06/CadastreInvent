using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Valuation
{
    public class ValuationAppealConfiguration : IEntityTypeConfiguration<ValuationAppeal>
    {
        public void Configure(EntityTypeBuilder<ValuationAppeal> builder)
        {
            builder.ToTable("valuation_appeals", "valuation");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ValuationId).IsRequired();
            builder.Property(x => x.ApplicantPartyId).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.SubmissionDate).IsRequired();

            builder.HasOne<CadastreInvent.Valuation.Domain.Entities.Valuation>()
                   .WithMany()
                   .HasForeignKey(x => x.ValuationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Party>()
                   .WithMany()
                   .HasForeignKey(x => x.ApplicantPartyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}