using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProSpace.Infrastructure.Entites.Users;

namespace ProSpace.Infrastructure.Configurations
{
    /// <summary>
    /// User configuration
    /// </summary>
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasMany(ur => ur.UserRoles)
                   .WithOne(u => u.UserApp)
                   .HasForeignKey(u => u.UserId)
                   .IsRequired();

            builder.HasOne(x => x.Customer)
                   .WithOne(u => u.AppUser)
                   .HasForeignKey<AppUser>(c => c.CustomerId)
                   .IsRequired(false);
        }
    }
}
