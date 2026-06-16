using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Domain.Models;
using ProSpace.Infrastructure.Mappers;

namespace ProSpace.Infrastructure.Repositories
{
    /// <summary>
    /// Order items repository
    /// </summary>
    public class OrderItemsRepository : IOrderItemsRepository
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
        public OrderItemsRepository(ILogger<OrderItemsRepository> logger, ProSpaceDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(OrderItemModel order, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = order.ToEntity();

                await _dbContext.OrderItems.AddAsync(entity, cancellationToken);

                _logger.LogInformation("Order item {Id} attached to the context for creation", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error adding order item {Id} to DB context", order.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.OrderItems
                         .AsNoTracking()
                         .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Delete failed. Order item with ID {Id} not found", id);
                    throw new KeyNotFoundException($"Order item with id = {id} was not found.");
                }

                _dbContext.OrderItems.Remove(found);
                _logger.LogInformation("Order item with ID {Id} marked for deletion in memory context", id);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Critical error deleting order item {Id} in DB context", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderItemModel[]> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var orders = await _dbContext.OrderItems
                          .AsNoTracking()
                          .Select(o => o.ToModel())
                          .ToArrayAsync(cancellationToken);

                _logger.LogInformation("Total order items loaded: {Length}", orders.Length);

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading list of all order items from database");

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.OrderItems
                         .AsNoTracking()
                         .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Cannot find order items with id = {id}", id);
                    return null;
                }

                return found.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error reading order items {Id} in DB context", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(OrderItemModel order, CancellationToken cancellationToken = default)
        {
            var found = await _dbContext.OrderItems.FindAsync([order.Id], cancellationToken)
                ?? throw new KeyNotFoundException($"Order with id = {order.Id} not found.");

            found.ItemPrice = order.ItemPrice;
            found.ItemId = order.ItemId;
            found.OrderId = order.OrderId;
            found.ItemsCount = order.ItemsCount;
        }

        /// <inheritdoc/>
        public async Task<OrderItemModel[]?> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var orders = await _dbContext.OrderItems
                 .AsNoTracking()
                 .Where(o => o.OrderId == orderId)
                 .Select(o => o.ToModel())
                 .ToArrayAsync(cancellationToken);

                _logger.LogInformation("Total order items loaded for order id {OrderId}: {Length}", orderId, orders.Length);

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading order items for customer code {OrderId}", orderId);
                throw;

                throw;
            }
        }
    }
}
