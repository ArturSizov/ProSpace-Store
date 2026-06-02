using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ProSpace.Infrastructure.Entites.Supply;

namespace ProSpace.Infrastructure.Configurations
{
    /// <summary>
    /// Item configuration
    /// </summary>
    public class ItemConfiguration : IEntityTypeConfiguration<ItemEntity>
    {
        public void Configure(EntityTypeBuilder<ItemEntity> builder)
        {
            builder.HasKey(o => o.Id);
        }
    }
}
