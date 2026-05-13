using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Registry
{
    public class PartyGroupConfiguration : IEntityTypeConfiguration<PartyGroup>
    {
        public void Configure(EntityTypeBuilder<PartyGroup> builder)
        {
            builder.ToTable("party_groups", "registry");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(255).IsRequired();

            builder.Property<uint>("xmin")
                   .HasColumnType("xid")
                   .ValueGeneratedOnAddOrUpdate()
                   .IsConcurrencyToken();

            builder.OwnsMany(x => x.Members, mb =>
            {
                mb.ToTable("party_group_members", "registry");
                mb.WithOwner().HasForeignKey(m => m.PartyGroupId);
                mb.HasKey(m => new { m.PartyGroupId, m.PartyId });
                mb.Property(m => m.PartyGroupId).ValueGeneratedNever();
                mb.Property(m => m.PartyId).ValueGeneratedNever();
                mb.Property(m => m.ShareNumerator).HasPrecision(18, 4).IsRequired();
                mb.Property(m => m.ShareDenominator).HasPrecision(18, 4).IsRequired();
            });

            builder.Metadata.FindNavigation(nameof(PartyGroup.Members))!
                   .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}