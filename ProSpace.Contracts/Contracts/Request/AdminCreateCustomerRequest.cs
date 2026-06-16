namespace ProSpace.Api.Contracts.Request
{
    public class AdminCreateCustomerRequest
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string UserCode { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public decimal Discount { get; init; }
    }
}
