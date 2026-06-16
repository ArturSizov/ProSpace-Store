using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProSpace.Infrastructure.Mappers;
using ProSpace.Application.Interfaces.Repositories;
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
        public async Task CreateAsync(OrderModel order, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = order.ToEntity();

                await _dbContext.Orders.AddAsync(entity, cancellationToken);

                _logger.LogInformation("Order {Id} attached to the context for creation", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error adding order {Id} to DB context", order.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Orders
                         .AsNoTracking()
                         .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Delete failed. Order with ID {Id} not found", id);
                    throw new KeyNotFoundException($"Order with id = {id} was not found.");
                }

                _dbContext.Orders.Remove(found);
                _logger.LogInformation("Order with ID {Id} marked for deletion in memory context", id);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Critical error deleting order {Id} in DB context", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel[]> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var orders = await _dbContext.Orders
                          .AsNoTracking()
                          .Select(o => o.ToModel())
                          .ToArrayAsync(cancellationToken);

                _logger.LogInformation("Total orders loaded: {Length}", orders.Length);

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading list of all orders from database");

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Orders
                         .AsNoTracking()
                         .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Cannot find orders with id = {id}", id);
                    return null;
                }

                return found.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error reading order {Id} in DB context", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(OrderModel order, CancellationToken cancellationToken = default)
        {
            var found = await _dbContext.Orders.FindAsync([order.Id], cancellationToken) 
                ?? throw new KeyNotFoundException($"Order with id = {order.Id} not found.");

            found.CustomerId = order.CustomerId;
            found.OrderDate = order.OrderDate;
            found.ShipmentDate = order.ShipmentDate;
            found.OrderNumber = order.OrderNumber;
            found.Status = order.Status;

        }

        /// <inheritdoc/>
        public async Task<OrderModel[]> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default)
        {
            try
            {
                var orders = await _dbContext.Orders
                    .AsNoTracking()
                    .Where(o => o.Customer.Code.Equals(customerCode, StringComparison.CurrentCultureIgnoreCase))
                    .Select(o => o.ToModel())
                    .ToArrayAsync(cancellationToken);

                _logger.LogInformation("Total orders loaded for customer {CustomerCode}: {Length}", customerCode, orders.Length);

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading orders for customer code {CustomerCode}", customerCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel[]> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var orders = await _dbContext.Orders
                   .AsNoTracking()
                   .Where(o => o.CustomerId == customerId)
                   .Select(o => o.ToModel())
                   .ToArrayAsync(cancellationToken);

                _logger.LogInformation("Total orders loaded for customer {CustomerId}: {Length}", customerId, orders.Length);

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading orders for customer ID {CustomerId}", customerId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel?> GetByOrderNumberAsync(int orderNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Cannot find orders with number = {number}", orderNumber);
                    return null;
                }

                return found.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error reading order {Number} in DB context", orderNumber);
                throw;
            }
        }
    }
}
