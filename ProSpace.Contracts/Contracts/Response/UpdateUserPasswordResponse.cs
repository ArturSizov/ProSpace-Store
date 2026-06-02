namespace ProSpace.Api.Contracts.Response
{
    /// <summary>
    /// User reset password response
    /// </summary>
    public class UpdateUserPasswordResponse
    {
        public required string Password { get; set; } = null!;

        public required string NewPassword { get; set; } = null!;
    }
}
