using ProSpace.Api.Contracts.Request;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Application.Interfaces.Services
{
    /// <summary>
    /// Defines business logic operations for managing customers and their accounts using a unified generic response.
    /// </summary>
    public interface ICustomersService
    {
        /// <summary>
        /// Registers a new customer in the system, creating both an Identity account and a profile.
        /// </summary>
        /// <param name="dto">The user registration dto containing credentials and profile data.</param>
        /// <param name="role">The security role assigned to the new user. Defaults to "customer".</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> indicating success with the new ID, or containing errors.</returns>
        Task<BaseIdResponse> RegisterCustomerAsync(RegisterCustomerDto dro, string role = "customer", CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of all customers registered in the system.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{T}"/> containing the collection of customer DTOs in the Data field.</returns>
        Task<BaseResponse<IEnumerable<CustomerDto>>> ReadAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the profile details of a specific customer by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the customer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{CustomerDto}"/> with the customer data in the Data field.</returns>
        Task<BaseResponse<CustomerDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the profile information of an existing customer.
        /// </summary>
        /// <param name="customer">The data transfer object containing the updated customer information (must include a valid Id).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{CustomerDto}"/> containing the updated profile data in the Data field.</returns>
        Task<BaseResponse<CustomerDto>> UpdateAsync(CustomerDto customer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for and returns a customer profile using their unique email address.
        /// </summary>
        /// <param name="email">The email address linked to the customer's Identity account.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{CustomerDto}"/> with the matching customer details in the Data field.</returns>
        Task<BaseResponse<CustomerDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completely deletes a customer profile from the database and removes their associated Identity account.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the customer to be deleted.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> indicating whether the deletion step was fully completed via the Id field.</returns>
        Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
