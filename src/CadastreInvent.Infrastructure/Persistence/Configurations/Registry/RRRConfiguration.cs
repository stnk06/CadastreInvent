using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Registry
{
    public class RRRConfiguration : IEntityTypeConfiguration<RRR>
    {
        public void Configure(EntityTypeBuilder<RRR> builder)
        {
            builder.ToTable("rrrs", "registry");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasConversion<string>().IsRequired();
            builder.Property(x => x.BAUnitId).IsRequired();
            builder.Property(x => x.SourceId).IsRequired();
            builder.Property(x => x.ShareNumerator).HasPrecision(18, 4).IsRequired();
            builder.Property(x => x.ShareDenominator).HasPrecision(18, 4).IsRequired();
            builder.Property(x => x.StartDate).IsRequired();
            builder.Property(x => x.EndDate);

            builder.Property<DateTime>("ValidFrom").IsRequired();
            builder.Property<DateTime>("ValidTo").IsRequired();

            builder.HasIndex(x => x.BAUnitId);
            builder.HasIndex(x => x.PartyId);
        }
    }
}