using ProSpace.Domain.Models;

namespace ProSpace.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines a specialized contract for order line items operations, extending the baseline generic 
    /// CRUD operations with aggregated queries linked to parent orders and customer profiles.
    /// </summary>
    public interface IOrderItemsRepository : IBasicCRUD<OrderItemModel, Guid>
    {
        /// <summary>
        /// Compiles a collection sequence of individual line items configured under a parent order aggregation root.
        /// </summary>
        /// <param name="orderId">The unique parent transaction group lookup tracker node context identifier value.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>A collection sequence tracking all loaded order item records matching the target order constraints.</returns>
        Task<IEnumerable<OrderItemModel>> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all order line items belonging to a specific customer by joining with the underlying active orders schema tables.
        /// </summary>
        /// <param name="customerId">The unique global identifier of the target customer whose purchase components are being extracted.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>A collection sequence of order item domain models matching the customer identity constraints context.</returns>
        Task<IEnumerable<OrderItemModel>> GetOrderItemsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    }
}
