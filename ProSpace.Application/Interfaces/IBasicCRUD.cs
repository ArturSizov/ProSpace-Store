namespace ProSpace.Application.Interfaces
{
    public interface IBasicCRUD<TModel, TKey> where TModel : class
    {
        /// <summary>
        /// Create item
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CreateAsync(TModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Read one item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TModel?> ReadAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update item
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task UpdateAsync(TModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete one item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Read all items
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TModel[]> ReadAllAsync(CancellationToken cancellationToken = default);
    }
}

