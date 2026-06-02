namespace ProSpace.Api.Contracts.Request
{
    /// <summary>
    /// User request
    /// </summary>
    public class UserRequest 
    {
        public required string Email { get; set; } = null!;
        public required string Password { get; set; } = null!;
        public required string UserName { get; set; } = null!;
        public required string UserCode { get; set; } = null!;
        public string? Address { get; set; }
        public decimal? Discount { get; set; }
    }
}
