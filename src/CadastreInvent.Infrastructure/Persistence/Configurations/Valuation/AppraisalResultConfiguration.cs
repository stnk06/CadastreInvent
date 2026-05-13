using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Valuation.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Valuation
{
    public class AppraisalResultConfiguration : IEntityTypeConfiguration<AppraisalResult>
    {
        public void Configure(EntityTypeBuilder<AppraisalResult> builder)
        {
            builder.ToTable("appraisal_results", "valuation");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SpatialUnitId).IsRequired();

            builder.Property(x => x.CalculatedValue).HasColumnType("numeric(18,2)").IsRequired();

            builder.Property(x => x.ConfidenceScore).IsRequired();

            builder.Property(x => x.AppraisalDate).IsRequired();

            builder.Property(x => x.MlModelVersion).HasMaxLength(100).IsRequired();

            builder.HasIndex(x => x.SpatialUnitId);
        }
    }
}