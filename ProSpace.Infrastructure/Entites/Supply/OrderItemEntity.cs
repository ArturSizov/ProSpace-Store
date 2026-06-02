namespace ProSpace.Infrastructure.Entites.Supply
{
    /// <summary>
    /// Order item
    /// </summary>
    public class OrderItemEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public OrderEntity Order { get; set; } = null!;
        public Guid ItemId { get; set; }
        public ItemEntity Item { get; set; } = null!;
        public required int ItemsCount { get; set; }
        public required decimal ItemPrice { get; set; }
    }
}
