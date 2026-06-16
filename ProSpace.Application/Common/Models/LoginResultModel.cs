namespace ProSpace.Application.Common.Models
{
    /// <summary>
    /// A universal result model for authentication and Login operations
    /// </summary>
    public record LoginResultModel
    {
        /// <summary>
        /// Flag indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Authorized user ID
        /// </summary>
        public Guid? UserId { get; init; }

        /// <summary>
        /// Authorized user token
        /// </summary>
        public string? Token { get; init; }

        /// <summary>
        /// Token lifetime
        /// </summary>
        public DateTime? Expiration { get; init; }


        /// <summary>
        /// User roles
        /// </summary>
        public IList<string>? Roles { get; init; }

        /// <summary>
        /// List of text errors (filled if IsSuccess = false).
        /// </summary>
        public IEnumerable<string> Errors { get; init; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="token"></param>
        /// <param name="expiration"></param>
        /// <param name="roles"></param>
        /// <param name="userId"></param>
        /// <param name="errors"></param>
        private LoginResultModel(bool isSuccess, string? token, DateTime? expiration, IList<string>? roles, Guid? userId, IEnumerable<string> errors)
        {
            IsSuccess = isSuccess;
            Token = token;
            Expiration = expiration;
            Roles = roles;
            UserId = userId;
            Errors = errors;
        }

        /// <summary>
        /// Factory method for generating a response.
        /// </summary>
        public static LoginResultModel Success(string token, DateTime expiration, IList<string> roles, Guid userId)
             => new(true, token, expiration, roles, userId, Array.Empty<string>());

        /// <summary>
        /// Factory method for generating a response with a collection of errors.
        /// </summary>
        public static LoginResultModel Failure(IEnumerable<string> errors)
            => new(false, null, null, null, null, errors);

        /// <summary>
        /// A convenient overload for generating a response with a single text error.
        /// </summary>
        public static LoginResultModel Failure(string singleError)
            => new(false, null, null, null, null, [singleError]);
    }
}
