namespace ProSpace.Api.Contracts.Request
{
    /// <summary>
    /// Order item request
    /// </summary>
    public class OrderItemRequest
    {
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
        public int ItemsCount { get; set; }
        public decimal ItemPrice { get; set; }
    }
}
