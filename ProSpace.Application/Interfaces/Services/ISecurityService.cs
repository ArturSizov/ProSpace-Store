namespace ProSpace.Application.Interfaces.Services
{
    /// <summary>
    /// Defines centralized security verification mechanics tracking cross-tenant order data isolation constraints.
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// Validates whether the currently authenticated user has structural permissions to access resources 
        /// associated with a specific customer profile.
        /// </summary>
        /// <remarks>
        /// This method enforces strict data isolation boundaries (Multi-Tenancy / Resource Ownership). 
        /// Administrative accounts (Managers) bypass validation rules entirely. Standard customer accounts 
        /// are strictly restricted to data where their identity token matches the system registry configuration.
        /// </remarks>
        /// <param name="customerId">The unique identifier of the customer profile owning the target resource.</param>
        /// <param name="ct">The cancellation token to abort the asynchronous evaluation workflow.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>IsSuccess</c>: <c>true</c> if access is granted; otherwise, <c>false</c>.</description></item>
        /// <item><description><c>Error</c>: A localized technical string describing the reason for refusal, or empty on success.</description></item>
        /// </list>
        /// </returns>
        Task<(bool IsSuccess, string Error)> ValidateCustomerAccessAsync(Guid customerId, CancellationToken ct = default);

        /// <summary>
        /// Returns the user's status.
        /// </summary>
        /// <returns></returns>
        bool IsManager();

        /// <summary>
        /// Extracts the unique security account claim out of the active HTTP request session threads 
        /// and resolves the corresponding primary key identification Guid tracking reference for the active Customer profile.
        /// </summary>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>
        /// A task that represents the asynchronous resolution operation. The task result contains 
        /// the unique global tracking <see cref="Guid"/> belonging to the customer profile record layout, 
        /// or <see langword="null"/> if the session context is anonymous, the token payload is invalid, 
        /// or no customer profile mapping has been instantiated in the storage registers.
        /// </returns>
        Task<Guid?> GetCurrentCustomerIdAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts the email address string associated with the currently authenticated user from the active HTTP claims principal context.
        /// </summary>
        /// <remarks>
        /// This method performs an in-memory scan of standard JWT token claim footprints (handling both <see cref="ClaimTypes.Email"/> 
        /// and raw "email" payload keys) to avoid redundant infrastructure database roundtrips.
        /// </remarks>
        /// <returns>
        /// A <see cref="string"/> representing the user's registered email address if found; 
        /// otherwise, <c>null</c> if the user context is unauthenticated or the claim is missing.
        /// </returns>
        string? GetCurrentUserEmail();
    }
}
