using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Valuation.Domain.Entities;
using System;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Valuation
{
    public class MassAppraisalModelConfiguration : IEntityTypeConfiguration<MassAppraisalModel>
    {
        public void Configure(EntityTypeBuilder<MassAppraisalModel> builder)
        {
            builder.ToTable("mass_appraisal_models", "valuation");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Version)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Algorithm)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ModelData)
                .IsRequired()
                .HasDefaultValue(Array.Empty<byte>());

            builder.Property(x => x.MetricsJson)
                .IsRequired(false)
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");
        }
    }
}