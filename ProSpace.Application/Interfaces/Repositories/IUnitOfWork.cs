namespace ProSpace.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Items repository
        /// </summary>
        IItemsRepository ItemsRepository { get; }

        /// <summary>
        /// Order items repository
        /// </summary>
        IOrderItemsRepository OrderItemsRepository { get; }

        /// <summary>
        /// Orders repository
        /// </summary>
        IOrdersRepository OrdersRepository { get; }

        /// <summary>
        /// Customers repository
        /// </summary>
        ICustomersRepository CustomersRepository { get; }

        /// <summary>
        /// Saving to database
        /// </summary>
        /// <returns></returns>
        Task<bool> CompleteAsync(CancellationToken cancellation = default);
    }
}
