namespace ProSpace.Application.Interfaces
{
    /// <summary>
    /// Defines a generic, reusable contract architecture executing basic Create, Read, Update, and Delete (CRUD) data persistence operations.
    /// </summary>
    /// <typeparam name="TModel">The domain framework data model structure type. Must be a reference type.</typeparam>
    /// <typeparam name="TKey">The primary key database datatype format used for record structural tracking.</typeparam>
    public interface IBasicCRUD<TModel, TKey> where TModel : class
    {
        /// <summary>
        /// Stages and persists a new domain model entry sequence into the underlying database infrastructure storage registers.
        /// </summary>
        /// <param name="model">The fully configured domain data model instance tracking information payload properties to store.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>A asynchronous execution <see cref="Task"/> tracking block verification state parameters.</returns>
        Task CreateAsync(TModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Locates and retrieves a single active domain model record utilizing its core primary key identifier lookup property.
        /// </summary>
        /// <param name="id">The explicit global storage primary key trace lookup index identity matching the database record.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>The matching translated domain model object context layout, or <see langword="null"/> if no matching record trace is located.</returns>
        Task<TModel?> ReadAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronizes and overrides mutable parameters of a target persistent database entry with modified domain model data properties.
        /// </summary>
        /// <param name="model">The updated domain data model tracking structural entity adjustments state updates to save.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>A asynchronous execution <see cref="Task"/> tracking block verification state parameters.</returns>
        Task UpdateAsync(TModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Permanently purges a definitive structural data item entry context boundary out of active persistent database records cache files.
        /// </summary>
        /// <param name="id">The unique targeted primary key identifier index trace node assigned for storage extraction tasks.</param>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>A asynchronous execution <see cref="Task"/> tracking block verification state parameters.</returns>
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Compiles a flat collection sequence mapping out all active registered domain models currently stored within the database schema layer.
        /// </summary>
        /// <param name="cancellationToken">An operational system token framework alerting for thread execution cancellation signals.</param>
        /// <returns>A collection sequence tracking all loaded domain model records inside an optimized read-only memory stream loop.</returns>
        Task<IEnumerable<TModel>> ReadAllAsync(CancellationToken cancellationToken = default);
    }
}
