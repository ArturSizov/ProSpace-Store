namespace ProSpace.Api.Contracts.Response
{
    /// <summary>
    /// Order response
    /// </summary>
    public class OrderResponse
    {
        public Guid CustomerId { get; set; }
        public DateOnly OrderDate { get; set; }
        public DateOnly? ShipmentDate { get; set; }
        public int? OrderNumber { get; set; }
        public string? Status { get; set; }
    }
}
