namespace ProSpace.Application.Interfaces.Services
{
    /// <summary>
    /// Defines centralized security verification mechanics tracking cross-tenant order data isolation constraints.
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// Validates whether the currently authenticated user has rights to modify or view data tied to a specific Customer ID.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer owning the target resource.</param>
        /// <returns>A tuple tracking success status alongside an optional validation failure description string.</returns>
        Task<(bool IsSuccess, string Error)> ValidateOrderOwnershipAsync(Guid customerId);

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
    }
}
