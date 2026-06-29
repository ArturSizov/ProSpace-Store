using ProSpace.Domain.Models;
using System.Linq.Expressions;

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

        /// <summary>
        /// Transitionally flags a customer profile record as soft-deleted within the structural data registries.
        /// </summary>
        /// <remarks>
        /// This operation mutates the deletion tracking flag rather than performing a physical destructive erasure. 
        /// In combination with global query filters, it safely isolates the profile from standard application queries 
        /// while preserving foreign key relational integrity across transactional historical ledgers.
        /// </remarks>
        /// <param name="id">The unique Guid tracking key of the targeted customer resource.</param>
        /// <param name="ct">The cancellation token to abort the asynchronous data modification routine.</param>
        Task SoftDeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Asynchronously generates the next sequential unique customer corporate identifier code.
        /// </summary>
        /// <remarks>
        /// This method scans the existing database records to find the maximum sequential numeric suffix, 
        /// increments it by 1, and formats the result into a standardized <c>0000-0001</c> string schema.
        /// It is designed to safely execute calculations entirely on the database side or via low-overhead 
        /// localized memory parsing to prevent race conditions during customer signup workflows.
        /// </remarks>
        /// <param name="ct">The cancellation token to abort the asynchronous database aggregation routine.</param>
        /// <returns>
        /// A <see cref="string"/> representing the newly generated, formatted unique sequence code (e.g., "0000-0006").
        /// Returns "0000-0001" if the database registry contains no existing records.
        /// </returns>
        Task<string> GenerateNextCustomerCodeAsync(CancellationToken ct = default);

        /// <summary>
        /// Checks if a customer code is already assigned to any profile other than the specified one.
        /// </summary>
        Task<bool> IsCodeAssignedToAnotherCustomerAsync(string code, Guid excludingCustomerId, CancellationToken ct);

    }
}
