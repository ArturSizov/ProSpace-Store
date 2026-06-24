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
        public async Task CreateAsync(OrderItemModel orderItem, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating database insertion sequence for a new Order Item entity record: {Id}", orderItem.Id);

                var entity = orderItem.ToEntity();
                await _dbContext.OrderItems.AddAsync(entity, cancellationToken);

                _logger.LogInformation("Order item {Id} successfully attached to the database memory context. Awaiting transaction commit.", orderItem.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while adding a new Order Item to DB context for ID: {Id}", orderItem.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to locate and extract Order Item profile for permanent removal: {Id}", id);

                var foundEntity = await _dbContext.OrderItems
                    .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogWarning("Extraction operation aborted. Order item with identity tracking token {Id} was not located.", id);
                    throw new KeyNotFoundException($"Order item with id = {id} was not found in the persistent storage context.");
                }

                _dbContext.OrderItems.Remove(foundEntity);
                _logger.LogInformation("Order item with ID {Id} successfully marked for removal in memory context. Awaiting commit.", id);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while executing order item destruction sequence for ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderItemModel>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating database schema scan to compile global order items registry report.");

                var entities = await _dbContext.OrderItems
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return entities.Select(entity => entity.ToModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical data layer disruption encountered while compiling the complete list of order items.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Scanning database schema for single order item record matching tracking identity: {Id}", id);

                var foundEntity = await _dbContext.OrderItems
                    .AsNoTracking() // Performance best practice for read-only single entry lookups
                    .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogInformation("No single order item record matches the requested database tracking identifier: {Id}", id);
                    return null;
                }

                return foundEntity.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while reading single order item for tracking identity: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(OrderItemModel orderItem, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to locate and update persistent state parameters for Order Item: {Id}", orderItem.Id);

                var foundEntity = await _dbContext.OrderItems.FindAsync([orderItem.Id], cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogWarning("Data modification aborted. Order item with tracking ID {Id} was not found inside registers.", orderItem.Id);
                    throw new KeyNotFoundException($"Order item with id = {orderItem.Id} not found in the persistent storage context.");
                }

                foundEntity.OrderId = orderItem.OrderId;
                foundEntity.ItemId = orderItem.ItemId;
                foundEntity.ItemsCount = orderItem.ItemsCount;
                foundEntity.ItemPrice = orderItem.ItemPrice;

                _dbContext.OrderItems.Update(foundEntity);

                _logger.LogInformation("Order item {Id} parameters successfully synchronized in-memory. Awaiting transaction commit.", orderItem.Id);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while updating state parameters for Order Item: {Id}", orderItem.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderItemModel>> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Compiling database order items collection for parent Order: {OrderId}", orderId);

                var entities = await _dbContext.OrderItems
                    .AsNoTracking()
                    .Where(item => item.OrderId == orderId)
                    .ToListAsync(cancellationToken);

                return entities.Select(entity => entity.ToModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer error occurred while fetching order items for Order: {OrderId}", orderId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderItemModel>> GetOrderItemsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Compiling database order items collection for Customer: {CustomerId}", customerId);

                var entities = await _dbContext.OrderItems
                    .AsNoTracking()
                    .Where(item => _dbContext.Orders.Any(order => order.Id == item.OrderId && order.CustomerId == customerId))
                    .ToListAsync(cancellationToken);

                return entities.Select(entity => entity.ToModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer error occurred while fetching order items for Customer: {CustomerId}", customerId);
                throw;
            }
        }
    }
}
