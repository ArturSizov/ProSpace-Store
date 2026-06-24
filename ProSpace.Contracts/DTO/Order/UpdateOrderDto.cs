namespace ProSpace.Contracts.DTO.Order
{
    public class UpdateOrderDto
    {
        public Guid Id { get; set; }
        public DateOnly? ShipmentDate { get; set; }
        public string? Status { get; set; }
    }
}
