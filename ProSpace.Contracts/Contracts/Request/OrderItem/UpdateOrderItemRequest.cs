namespace ProSpace.Contracts.Contracts.Request.OrderItem
{
    public class UpdateOrderItemRequest
    {
        public Guid ItemId { get; set; }
        public int ItemsCount { get; set; }
    }
}
