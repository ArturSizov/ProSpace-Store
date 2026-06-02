namespace ProSpace.Api.Contracts.Response
{
    /// <summary>
    /// Order item response
    /// </summary>
    public class OrderItemResponse
    {
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
        public int ItemsCount { get; set; }
        public decimal ItemPrice { get; set; }
    }
}
