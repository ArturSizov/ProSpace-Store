namespace ProSpace.Api.Contracts.Request
{
    /// <summary>
    /// Order request
    /// </summary>
    public class OrderRequest
    {
        public Guid CustomerId { get; set; }
        public DateOnly OrderDate { get; set; }
        public DateOnly? ShipmentDate { get; set; }
        public int? OrderNumber { get; set; }
        public string? Status { get; set; }
    }
}
