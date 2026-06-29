namespace ProSpace.Contracts.DTO.Customer
{
    public class RegisterCustomerDto
    {
        public string Email { get; init; } = null!;
        public string Password { get; init; } = null!;
        public string UserName { get; init; } = null!;
        public string? Address { get; init; }
        public decimal Discount { get; init; }
    }
}
