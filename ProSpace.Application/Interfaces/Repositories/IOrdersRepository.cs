using ProSpace.Domain.Models;

namespace ProSpace.Application.Interfaces.Repositories
{
    public interface IOrdersRepository : IBasicCRUD<OrderModel, Guid>
    {
        /// <summary>
        /// Receives orders by customer ID
        /// </summary>
        /// <param name="custonerId"></param>
        /// <returns></returns>
        Task<OrderModel[]> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives orders by customer code
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        Task<OrderModel[]> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives an order by order number
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        Task<OrderModel?> GetByOrderNumberAsync(int orderNumber, CancellationToken cancellationToken = default);
    }
}
