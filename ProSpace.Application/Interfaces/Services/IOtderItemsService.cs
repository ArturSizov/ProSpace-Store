using ProSpace.Contracts.DTO.OrderItem;
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
        Task<BaseIdResponse> CreateAsync(CreateOrderItemDto orderItem, CancellationToken cancellationToken = default);

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
        /// Completely removes an item from an order and updates the context.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order item to be deleted.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> indicating whether the deletion step was fully completed via the Id field.</returns>
        Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously synchronizes and updates an existing order item record inside persistent storage registries.
        /// </summary>
        /// <remarks>
        /// This serves as a unified orchestration pipeline handling both administrative overrides and standard customer mutations:
        /// <list type="bullet">
        /// <item><description>Enforces strict multi-tenancy boundaries via <c>ValidateCustomerAccessAsync</c> to prevent IDOR vulnerabilities.</description></item>
        /// <item><description>For Managers: Bypasses lifecycle constraints and applies administrative price parameters if explicitly provided (non-zero).</description></item>
        /// <item><description>For Customers: Restricts modifications to orders with a "New" status and forces an automated price recalculation using current catalog rates and active profile discounts to prevent fraud.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="dto">The unified data transfer object containing the mutated entity state parameters.</param>
        /// <param name="cancellationToken">The cancellation token to abort the asynchronous database transaction sequence.</param>
        /// <returns>A standard service response wrapping the updated <see cref="OrderItemDto"/> representation payload.</returns>

        Task<BaseResponse<OrderItemDto>> UpdateAsync(UpdateOrderItemDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all individual items belonging to a specific order.
        /// </summary>
        /// <param name="orderId">The unique identifier (GUID) of the parent order.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{OrderItemDto}"/> containing the array of order items within the Data field.</returns>
        Task<BaseResponse<IEnumerable<OrderItemDto>>> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
