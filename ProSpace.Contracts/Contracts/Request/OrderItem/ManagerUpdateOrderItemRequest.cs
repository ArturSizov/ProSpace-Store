namespace ProSpace.Contracts.Contracts.Request.OrderItem
{
    public class ManagerUpdateOrderItemRequest
    {
        public Guid ItemId { get; set; }
        public int ItemsCount { get; set; }
        public decimal ItemPrice { get; set; }
    }
}
