using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProSpace.Infrastructure.Mappers;
using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Domain.Models;

namespace ProSpace.Infrastructure.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        /// <summary>
        /// Logger 
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Db context
        /// </summary>
        private readonly ProSpaceDbContext _dbContext;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        public OrdersRepository(ILogger<OrdersRepository> logger, ProSpaceDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<(OrderModel?, IDictionary<string, string[]>?)> CreateAsync(OrderModel entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = entity.ToEntity();
                var result = await _dbContext.Orders.AddAsync(order, cancellationToken);
                var saved = await _dbContext.SaveChangesAsync(cancellationToken);

                if (saved > 0)
                    return (result.Entity.ToModel(), null);

                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot create an order");
                return (null, null );
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Orders.FindAsync([id], cancellationToken);
                if (found == null)
                {
                    _logger.LogWarning("Cannot find order with id = {id}", id);
                    return false;
                }

                _ = _dbContext.Orders.Remove(found);

                var saved = await _dbContext.SaveChangesAsync(cancellationToken);

                return saved > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot delete order");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel[]?> GetByCustomerCode(string customerCode)
        {
            try
            {
                var orders = await _dbContext.Orders
                                   .Where(o => o.Customer.Code == customerCode)
                                   .ToListAsync();

                return orders.Select(x => x.ToModel()).ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel[]?> GetByCustomerId(Guid customerId)
        {
            try
            {
                var orders = await _dbContext.Orders
                                   .Where(o => o.Customer.Id == customerId)
                                   .ToListAsync();

                return orders.Select(x => x.ToModel()).ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel?> GetByOrderNumber(int orderNumber)
        {
            try
            {
                var orderEntity = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                return orderEntity?.ToModel() ?? throw new Exception("Order is null");
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel[]?> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var orders = await _dbContext.Orders.ToArrayAsync();

                return orders.Select(x => x.ToModel()).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot read all orders");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Orders.FindAsync([id], cancellationToken);
                if (found == null)
                {
                    _logger.LogWarning("Cannot find order with id = {id}", id);
                    return null;
                }

                return found.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot find order");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel?> UpdateAsync(OrderModel entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Orders.FindAsync([entity.Id], cancellationToken);
                if (found == null)
                {
                    _logger.LogWarning("Cannot find order with id = {id}", entity.Id);
                    return null;
                }

                found.OrderNumber = entity.OrderNumber;
                found.OrderDate = entity.OrderDate;
                found.CustomerId = entity.CustomerId;
                found.Status = entity.Status;
                found.ShipmentDate = entity.ShipmentDate;

                _ = _dbContext.Orders.Update(found);

                var saved = await _dbContext.SaveChangesAsync(cancellationToken);
                if (saved > 0)
                    return found.ToModel();

                _logger.LogError("Cannot save the updated data");
                return null;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot update order");
                return null;
            }
        }
    }
}
