using ProSpace.Domain.Models;

namespace ProSpace.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines a specialized contract for customer-specific data operations, extending the baseline 
    /// generic CRUD operations with custom identity and credentials lookup parameters.
    /// </summary>
    public interface ICustomersRepository : IBasicCRUD<CustomerModel, Guid>
    {
        /// <summary>
        /// Locates and retrieves a single customer profile utilizing their unique linked corporate email address.
        /// </summary>
        /// <param name="email">The unique string identity email address tracking parameters to look up in the database schema.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>The matching customer profile domain model layout, or <see langword="null"/> if no matching record trace is located.</returns>
        Task<CustomerModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a customer profile mapping from the persistent storage using the linked authorization system identity account user identifier.
        /// </summary>
        /// <param name="appUserId">The unique global identity database tracking key belonging to the authenticated system account profile.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>The matching customer profile domain model framework structure, or <see langword="null"/> if no tracking trace matches.</returns>
        Task<CustomerModel?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default);
    }
}
