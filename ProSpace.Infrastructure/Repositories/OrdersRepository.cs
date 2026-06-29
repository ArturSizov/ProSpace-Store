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
                _logger.LogInformation("Initiating database insertion sequence for a new Order entity record: {Id}", order.Id);

                var entity = order.ToEntity();

                await _dbContext.Orders.AddAsync(entity, cancellationToken);

                _logger.LogInformation("Order {Id} successfully attached to the database memory context. Awaiting transaction commit.", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while adding a new Order to DB context for ID: {Id}", order.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to locate and extract Order profile for permanent removal: {Id}", id);

                var foundEntity = await _dbContext.Orders
                    .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogWarning("Extraction operation aborted. Order with identity tracking token {Id} was not located.", id);
                    throw new KeyNotFoundException($"Order with id = {id} was not found in the persistent storage context.");
                }

                _dbContext.Orders.Remove(foundEntity);

                _logger.LogInformation("Order with ID {Id} successfully marked for removal in memory context. Awaiting transaction commit.", id);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while executing order destruction sequence for ID: {Id}", id);

            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderModel>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating database schema scan to compile global order items registry report.");

                var entities = await _dbContext.Orders
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var orderModelsList = entities.Select(entity => entity.ToModel());

                _logger.LogInformation("Global database scan finalized successfully. Total orders loaded into context: {Count}", entities.Count);

                return orderModelsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical data layer disruption encountered while compiling the complete list of orders.");

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Scanning database schema for single order record matching tracking identity: {Id}", id);

                var foundEntity = await _dbContext.Orders
                    .AsNoTracking() 
                    .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogInformation("No single order record matches the requested database tracking identifier: {Id}", id);
                    return null;
                }

                return foundEntity.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while reading single order for tracking identity: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(OrderModel order, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to locate and update persistent state parameters for Order: {Id}", order.Id);

                var foundEntity = await _dbContext.Orders.FindAsync([order.Id], cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogWarning("Data modification aborted. Order with tracking ID {Id} was not found inside the database registers.", order.Id);
                    throw new KeyNotFoundException($"Order with id = {order.Id} not found in the persistent storage context.");
                }

                foundEntity.CustomerId = order.CustomerId;
                foundEntity.OrderDate = order.OrderDate;
                foundEntity.ShipmentDate = order.ShipmentDate;
                foundEntity.OrderNumber = order.OrderNumber;
                foundEntity.Status = order.Status;

                _dbContext.Orders.Update(foundEntity);

                _logger.LogInformation("Order {Id} parameters successfully synchronized in-memory. Awaiting transaction commit.", order.Id);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while updating state parameters for Order: {Id}", order.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderModel>> GetOrdersByCustomerCodeAsync(string customerCode, Guid? customerId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Scanning orders registry for records linked to customer corporate code: {CustomerCode}", customerCode);

                var query = _dbContext.Orders
                    .AsNoTracking()
                    .Where(order => _dbContext.Customers.Any(customer =>
                        customer.Id == order.CustomerId &&
                        customer.Code == customerCode));

                if (customerId.HasValue && customerId.Value != Guid.Empty)
                    query = query.Where(order => order.CustomerId == customerId.Value);


                var entities = await query.ToListAsync(cancellationToken);

                _logger.LogInformation("Total order items successfully loaded for customer code {CustomerCode}: {Count}", customerCode, entities.Count);

                return entities.Select(entity => entity.ToModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical data layer disruption encountered while reading orders for customer code: {CustomerCode}", customerCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderModel>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Scanning database schema for orders linked to Customer ID: {CustomerId}", customerId);

                var entities = await _dbContext.Orders
                    .AsNoTracking()
                    .Where(order => order.CustomerId == customerId)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Total orders successfully loaded for Customer ID {CustomerId}: {Count}", customerId, entities.Count);

                return entities.Select(entity => entity.ToModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical data layer disruption encountered while reading orders for Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OrderModel?> GetByOrderNumberAsync(int orderNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Scanning database schema for single order matching serial sequence number: {OrderNumber}", orderNumber);

                var foundEntity = await _dbContext.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(order => order.OrderNumber == orderNumber, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogInformation("No single order record matches the requested serial sequence number: {OrderNumber}", orderNumber);
                    return null;
                }

                return foundEntity.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while reading order by serial number: {OrderNumber}", orderNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetMaxOrderNumberAsync(CancellationToken ct)
            => await _dbContext.Orders.MaxAsync(o => o.OrderNumber, ct) ?? 0;


        /// <inheritdoc/>
        public async Task<OrderModel?> GetOrderByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Scanning database schema for single order record matching tracking identity: {CustometId}", customerId);

                var foundEntity = await _dbContext.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(order => order.CustomerId == customerId, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogInformation("No single order record matches the requested database tracking identifier: {CustometId}", customerId);
                    return null;
                }

                return foundEntity.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while reading single order for tracking identity: {CustometId}", customerId);
                throw;
            }
        }
    }
}
