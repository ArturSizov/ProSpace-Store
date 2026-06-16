using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Application.Interfaces.Services
{
    /// <summary>
    /// Defines business logic operations for managing order headers using a unified generic response framework.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order header in the system.
        /// </summary>
        /// <param name="order">The data transfer object containing the new order details.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> containing the operation status and the generated ID of the new order.</returns>
        Task<BaseIdResponse> CreateAsync(CreateOrderDto order, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific order header by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderDto}"/> with the order details inside the Data field, or a failure message if not found.</returns>
        Task<BaseResponse<OrderDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of all order headers registered in the system.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderDto}"/> containing the collection of all orders within the Data field.</returns>
        /// <remarks>
        /// WARNING: As the order volume grows, this method should be replaced 
        /// with a paginated approach to prevent high memory consumption.
        /// </remarks>
        Task<BaseResponse<OrderDto[]>> ReadAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the core details of an existing order header.
        /// </summary>
        /// <param name="order">The data transfer object containing updated details (must include a valid Id).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderDto}"/> indicating the update operation status and containing the updated data payload.</returns>
        Task<BaseResponse<OrderDto>> UpdateAsync(OrderDto order, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completely deletes an order header from the database.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order to be deleted.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> indicating whether the deletion step was fully completed via the Id field.</returns>
        Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all orders belonging to a specific customer using their unique identifier.
        /// </summary>
        /// <param name="customerId">The unique identifier (GUID) of the customer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderDto}"/> containing the array of orders within the Data field.</returns>
        Task<BaseResponse<OrderDto[]>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all orders belonging to a specific customer using their unique business code.
        /// </summary>
        /// <param name="customerCode">The unique business code string of the customer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderDto}"/> containing the array of orders within the Data field.</returns>
        Task<BaseResponse<OrderDto[]>> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single order profile using its unique human-readable order number.
        /// </summary>
        /// <param name="orderNumber">The sequential or unique integer number assigned to the order.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderDto}"/> with the order details inside the Data field, or a failure message if not found.</returns>
        Task<BaseResponse<OrderDto>> GetByOrderNumberAsync(int orderNumber, CancellationToken cancellationToken = default);
    }
}
