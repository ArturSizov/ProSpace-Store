namespace ProSpace.Infrastructure.Entites.Supply
{
    /// <summary>
    /// Order
    /// </summary>
    public class OrderEntity
    {
        public required Guid Id { get; set; }
        public required DateOnly OrderDate { get; set; }
        public DateOnly? ShipmentDate { get; set; }
        public int? OrderNumber { get; set; }
        public string? Status { get; set; }
        public required Guid CustomerId { get; set; }
        public CustomerEntity Customer { get; set; } = null!;
        public ICollection<OrderItemEntity> OrderItems { get; set; } = [];
    }

}
