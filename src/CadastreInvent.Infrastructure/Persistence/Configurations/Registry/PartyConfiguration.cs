using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Registry
{
    public class PartyConfiguration : IEntityTypeConfiguration<Party>
    {
        public void Configure(EntityTypeBuilder<Party> builder)
        {
            builder.ToTable("parties", "registry");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExtId).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
            builder.Property(x => x.Type).HasConversion<string>().IsRequired();
            builder.Property(x => x.ContactInfo).HasMaxLength(500);
        }
    }
}
