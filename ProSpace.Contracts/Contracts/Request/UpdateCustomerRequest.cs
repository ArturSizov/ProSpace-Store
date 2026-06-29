namespace ProSpace.Api.Contracts.Request
{
    public partial class UpdateCustomerRequest
    {
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
    }
}
