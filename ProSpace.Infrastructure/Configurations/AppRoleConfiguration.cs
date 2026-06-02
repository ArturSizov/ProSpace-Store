using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProSpace.Infrastructure.Entites.Users;

namespace ProSpace.Infrastructure.Configurations
{
    /// <summary>
    /// App role configuration
    /// </summary>
    public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
    {
        public void Configure(EntityTypeBuilder<AppRole> builder)
        {
            builder.HasMany(ur => ur.UserRoles)
                   .WithOne(u => u.RoleApp)
                   .HasForeignKey(u => u.RoleId)
                   .IsRequired();
        }
    }
}
