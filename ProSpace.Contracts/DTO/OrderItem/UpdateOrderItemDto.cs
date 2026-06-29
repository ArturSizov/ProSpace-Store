namespace ProSpace.Contracts.DTO.OrderItem
{
    public class UpdateOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public int ItemsCount { get; set; }
        public decimal ItemPrice { get; set; } = 0;
    }
}
