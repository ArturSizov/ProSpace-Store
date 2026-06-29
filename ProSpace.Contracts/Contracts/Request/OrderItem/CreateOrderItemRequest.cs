namespace ProSpace.Contracts.Contracts.Request.OrderItem
{
    /// <summary>
    /// Order item request
    /// </summary>
    public class CreateOrderItemRequest
    {
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
        public int ItemsCount { get; set; }
    }
}
