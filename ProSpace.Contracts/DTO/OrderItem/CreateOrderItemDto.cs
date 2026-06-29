namespace ProSpace.Contracts.DTO.OrderItem
{
    public class CreateOrderItemDto
    {
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
        public int ItemsCount { get; set; }
        public decimal ItemPrice { get; set; }
    }
}
