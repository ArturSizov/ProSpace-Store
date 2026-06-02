using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ProSpace.Infrastructure.Entites.Supply;
using ProSpace.Infrastructure.Entites.Users;

namespace ProSpace.Infrastructure.Configurations
{
    /// <summary>
    /// Customer configuration
    /// </summary>
    public class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
    {
        public void Configure(EntityTypeBuilder<CustomerEntity> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.HasOne(x => x.AppUser)
                    .WithOne(u => u.Customer)
                    .HasForeignKey<AppUser>(c => c.CustomerId)
                    .IsRequired();

            builder.HasMany(x => x.Orders)
                   .WithOne(o => o.Customer)
                   .HasForeignKey(o => o.CustomerId)
                   .IsRequired(false);
        }
    }
}
