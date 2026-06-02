using ProSpace.Domain.Interfaces.Repositories;

namespace ProSpace.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Data context
        /// </summary>
        private readonly ProSpaceDbContext _dataContext;

        /// <inheritdoc/>
        public IItemsRepository ItemsRepository { get; }

        /// <inheritdoc/>
        public IOrderItemsRepository OrderItemsRepository { get; }

        /// <inheritdoc/>
        public IOrdersRepository OrdersRepository { get; }

        /// <inheritdoc/>
        public ICustomersRepository CustomersRepository { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="itemsRepository"></param>
        public UnitOfWork(ProSpaceDbContext dataContext, IItemsRepository itemsRepository, 
            IOrderItemsRepository orderItemsRepository, 
            IOrdersRepository ordersRepository, ICustomersRepository customersRepository)
        {
            _dataContext = dataContext;

            ItemsRepository = itemsRepository;
            OrderItemsRepository = orderItemsRepository;
            OrdersRepository = ordersRepository;
            CustomersRepository = customersRepository;
        }

        /// <inheritdoc/>
        public async Task<bool> CompleteAsync()
        {
            try
            {
                return await _dataContext.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
