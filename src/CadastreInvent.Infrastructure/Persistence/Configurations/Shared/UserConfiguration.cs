using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Infrastructure.Persistence.Configurations.Shared
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users", "shared");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Username).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
            builder.HasIndex(x => x.Email).IsUnique();

            builder.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();

            builder.Property(x => x.RoleId).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();

            builder.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}