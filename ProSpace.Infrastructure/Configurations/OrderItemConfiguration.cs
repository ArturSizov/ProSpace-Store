using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ProSpace.Infrastructure.Entites.Supply;

namespace ProSpace.Infrastructure.Configurations
{
    /// <summary>
    /// Order item configuration
    /// </summary>
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
    {
        public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.HasOne(oi => oi.Order)
                   .WithMany(o => o.OrderItems)
                   .HasForeignKey(o => o.OrderId);

            builder.HasOne(oi => oi.Item)
                   .WithMany(i => i.OrderItems)
                   .HasForeignKey(o => o.ItemId);
        }
    }
}
