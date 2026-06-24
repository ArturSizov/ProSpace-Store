using ProSpace.Domain.Models;

namespace ProSpace.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines a specialized contract for order aggregation roots management, extending the baseline 
    /// generic CRUD operations with specialized queries tracking sequence numbers, unique codes, and customer bounds.
    /// </summary>
    public interface IOrdersRepository : IBasicCRUD<OrderModel, Guid>
    {
        /// <summary>
        /// Compiles a collection sequence of all active orders assigned underneath a specific customer primary key tracker.
        /// </summary>
        /// <param name="customerId">The unique global identifier (GUID) of the target customer profile account.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>A collection sequence tracking all loaded order records matching the target customer identity parameters.</returns>
        Task<IEnumerable<OrderModel>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

        Task<OrderModel?> GetOrderByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a collection sequence of all transaction orders mapped to a specific corporate customer system code.
        /// </summary>
        /// <param name="customerCode">The unique string tracking code descriptor assigned to the system buyer profile.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <param name="customerId">
        /// Optional customer identifier used to restrict data visibility. 
        /// If provided (non-null), the query restricts the results to this customer only (Client mode).
        /// If omitted (null), no user-scoping is applied, fetching all records matching the code (Manager mode).
        /// </param>
        /// <returns>A collection sequence tracking all loaded order records matching the target customer code context.</returns>
        Task<IEnumerable<OrderModel>> GetOrdersByCustomerCodeAsync(string customerCode, Guid? customerId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Locates and extracts a single order record utilizing its definitive unique sequential order number index.
        /// </summary>
        /// <param name="orderNumber">The system generated tracking sequence serial number assigned to the specific purchase lifecycle transaction.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>The matching order domain model configuration layout, or <see langword="null"/> if no matching transaction trace is located.</returns>
        Task<OrderModel?> GetByOrderNumberAsync(int orderNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves the highest sequential order number currently stored in the database.
        /// </summary>
        /// <remarks>
        /// This method executes an aggregate MAX function directly on the database server side via SQL.
        /// It prevents loading the entire orders table into the application's memory (RAM), ensuring high performance.
        /// It is primarily used to safely calculate the next unique order number (Max + 1) when database-side 
        /// identity generation is unavailable or restricted (e.g., when using SQLite with Guid primary keys).
        /// </remarks>
        /// <param name="ct">The cancellation token to abort the asynchronous operation.</param>
        /// <returns>
        /// The maximum order number found as an <see cref="int"/>. 
        /// Returns <c>0</c> if the database table contains no records or all existing numbers are null.
        /// </returns>
        Task<int> GetMaxOrderNumberAsync(CancellationToken ct);
    }
}