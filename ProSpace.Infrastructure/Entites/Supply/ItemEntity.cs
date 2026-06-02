using System.ComponentModel.DataAnnotations;

namespace ProSpace.Infrastructure.Entites.Supply
{
    /// <summary>
    /// Item/Product
    /// </summary>
    public class ItemEntity
    {
        public Guid Id { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        [MaxLength(30)]
        public string? Category { get; set; }
        public ICollection<OrderItemEntity> OrderItems { get; set; } = [];
    }

}
