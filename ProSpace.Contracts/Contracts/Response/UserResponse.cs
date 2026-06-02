namespace ProSpace.Api.Contracts.Response
{
    /// <summary>
    /// User response
    /// </summary>
    public class UserResponse
    {
        public required string Email { get; set; } = null!;
        public required string Password { get; set; } = null!;
    }
}
