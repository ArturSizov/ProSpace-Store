using ProSpace.Contracts.DTO.Order;
using ProSpace.Contracts.Responses;

namespace ProSpace.Application.Interfaces.Services
{
    /// <summary>
    /// Defines business logic operations for managing order headers using a unified generic response framework.
    /// Manages core transactional processing rules, auto-number generation, and data tenancy boundaries checks.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order header in the system.
        /// Automatically handles authoritative server-side numbers, status initialization, and identity locks.
        /// </summary>
        /// <param name="order">The data transfer object containing the new order details.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> containing the operation status and the generated ID of the new order.</returns>
        Task<BaseIdResponse> CreateAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific order header by its unique identifier.
        /// Enforces data isolation checks: managers can read any record; standard accounts read only their own orders.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{T}"/> where T is <see cref="OrderDto"/>, delivering properties within the Data field.</returns>
        Task<BaseResponse<OrderDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of all order headers registered in the system.
        /// Enforces multi-tenant separation: managers receive the global sequence log, customers receive their personal ledger.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{T}"/> where T is <see cref="IEnumerable{OrderDto}"/>, containing the filtered collection within the Data field.</returns>
        /// <remarks>
        /// WARNING: As the order volume grows, this method should be replaced 
        /// with a paginated approach to prevent high memory consumption.
        /// </remarks>
        Task<BaseResponse<IEnumerable<OrderDto>>> ReadAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the core details of an existing order header.
        /// Enforces ownership validation boundaries and field-level property updates protection blocks under roles.
        /// </summary>
        /// <param name="order">The data transfer object containing updated details (must include a valid Id).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{T}"/> where T is <see cref="OrderDto"/>, indicating status and wrapping the updated data payload.</returns>
        Task<BaseResponse<OrderDto>> UpdateAsync(UpdateOrderDto order, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completely deletes an order header from the database along with all its nested order line items components.
        /// Enforces role restrictions: clients can delete only unfulfilled 'New' orders; managers bypass workflow limits.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order to be deleted.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> indicating whether the deletion step was fully completed via the Id field.</returns>
        Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all orders belonging to a specific customer using their unique identifier.
        /// Enforces strict security: managers can view any account history, customers are restricted to matching their personal profile key.
        /// </summary>
        /// <param name="customerId">The unique identifier (GUID) of the customer profile record lookup target.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{T}"/> where T is <see cref="IEnumerable{OrderDto}"/>, containing the collection within the Data field.</returns>
        Task<BaseResponse<IEnumerable<OrderDto>>> GetOrderByCustomersIdAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all orders belonging to a specific customer using their unique corporate business code string.
        /// Enforces strict privacy parameters: standard users are blocked from querying code assignments of other accounts.
        /// </summary>
        /// <param name="customerCode">The unique business code string of the target customer account.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{T}"/> where T is <see cref="IEnumerable{OrderDto}"/>, containing the collection within the Data field.</returns>
        Task<BaseResponse<IEnumerable<OrderDto>>> GetByOrdersCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single order profile using its unique human-readable sequential tracking order number.
        /// Enforces tenancy validation checks: standard clients are blocked from inspecting data logs of other buyers transactions.
        /// </summary>
        /// <param name="orderNumber">The sequential or unique integer number assigned to the purchase lifecycle record.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{T}"/> where T is <see cref="OrderDto"/>, wrapping details inside the Data field or a failure packet if unauthorized.</returns>
        Task<BaseResponse<OrderDto>> GetByOrderNumberAsync(int orderNumber, CancellationToken cancellationToken = default);
    }
}
