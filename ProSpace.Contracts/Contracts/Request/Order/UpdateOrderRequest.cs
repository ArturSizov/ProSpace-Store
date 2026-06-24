namespace ProSpace.Contracts.Contracts.Request.Order
{
    public class UpdateOrderRequest
    {
        public DateOnly? ShipmentDate { get; set; }
        public string? Status { get; set; }
    }
}
