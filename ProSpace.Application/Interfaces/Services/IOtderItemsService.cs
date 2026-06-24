using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Application.Interfaces.Services
{
    /// <summary>
    /// Defines business logic operations for managing individual order items (order lines) using a unified generic response framework.
    /// </summary>
    public interface IOrderItemsService
    {
        /// <summary>
        /// Adds a new item/product to an existing order.
        /// </summary>
        /// <param name="orderItem">The data transfer object containing the order item details, including Quantity and Price.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> containing the operation status and the generated ID of the new order item.</returns>
        Task<BaseIdResponse> CreateAsync(OrderItemDto orderItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific order item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order item.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderItemDto}"/> with the order item details inside the Data field, or a failure message if not found.</returns>
        Task<BaseResponse<OrderItemDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of all order items registered in the system.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderItemDto}"/> containing the collection of all order items within the Items field.</returns>
        /// <remarks>
        Task<BaseResponse<IEnumerable<OrderItemDto>>> ReadAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the details (such as quantity or price) of an existing order item.
        /// </summary>
        /// <param name="orderItem">The data transfer object containing updated details (must include a valid Id).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderItemDto}"/> indicating the update operation status and containing the updated data payload.</returns>
        Task<BaseResponse<OrderItemDto>> UpdateAsync(OrderItemDto orderItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completely removes an item from an order and updates the context.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order item to be deleted.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> indicating whether the deletion step was fully completed via the Id field.</returns>
        Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all individual items belonging to a specific order.
        /// </summary>
        /// <param name="orderId">The unique identifier (GUID) of the parent order.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderItemDto}"/> containing the array of order items within the Data field.</returns>
        Task<BaseResponse<IEnumerable<OrderItemDto>>> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
