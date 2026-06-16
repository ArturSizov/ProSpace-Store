namespace ProSpace.Application.Common.Models
{
    /// <summary>
    /// A universal result model for authentication and Identity operations.
    /// Completely isolates the Application layer from the Microsoft.AspNetCore.Identity classes.
    /// </summary>
    public record IdentityResultModel
    {
        public bool IsSuccess { get; init; }
        public string? UserId { get; init; }
        public IEnumerable<string> Errors { get; init; }

        // A private constructor so that the object can only be created through factory methods.
        private IdentityResultModel(bool isSuccess, string? userId, IEnumerable<string> errors)
        {
            IsSuccess = isSuccess;
            UserId = userId;
            Errors = errors;
        }

        /// <summary>
        /// Returns a successful result with the ID of the created user.
        /// </summary>
        public static IdentityResultModel Success(string? userId)
            => new(true, userId, Array.Empty<string>());

        /// <summary>
        /// Returns a runtime error with a list of text descriptions.
        /// </summary>
        public static IdentityResultModel Failure(IEnumerable<string> errors)
            => new(false, null, errors);
    }
}
