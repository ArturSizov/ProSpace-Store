using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProSpace.Infrastructure.Mappers;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Domain.Models;

namespace ProSpace.Infrastructure.Repositories
{
    public class ItemsRepository : IItemsRepository
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
        public ItemsRepository(ILogger<ItemsRepository> logger, ProSpaceDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(ItemModel item, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating database insertion sequence for a new product Item entity record: {Id}", item.Id);

                var entity = item.ToEntity();

                await _dbContext.Items.AddAsync(entity, cancellationToken);

                _logger.LogInformation("Product item {Id} successfully attached to the database memory context. Awaiting transaction commit.", item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while adding a new product Item to DB context for ID: {Id}", item.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Scanning database schema for single product item record matching tracking identity: {Id}", id);

                var foundEntity = await _dbContext.Items
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogInformation("No single product item record matches the requested database tracking identifier: {Id}", id);
                    return null;
                }

                return foundEntity.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while reading single product item for tracking identity: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(ItemModel item, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to locate and update persistent state parameters for item: {Id}", item.Id);

                var foundEntity = await _dbContext.Items.FindAsync([item.Id], cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogWarning("Data modification aborted. Item with tracking ID {Id} was not found inside registers.", item.Id);
                    throw new KeyNotFoundException($"Item with id = {item.Id} not found in the persistent storage context.");
                }

                foundEntity.Name = item.Name;
                foundEntity.Code = item.Code;
                foundEntity.Price = item.Price;
                foundEntity.Category = item.Category;

                _dbContext.Items.Update(foundEntity);

                _logger.LogInformation("Item {Id} parameters successfully synchronized in-memory. Awaiting transaction commit.", item.Id);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while updating state parameters for Item: {Id}", item.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to locate and extract item profile for permanent removal: {Id}", id);

                var foundEntity = await _dbContext.Items
                    .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogWarning("Extraction operation aborted. Item with identity tracking token {Id} was not located.", id);
                    throw new KeyNotFoundException($"item with id = {id} was not found in the persistent storage context.");
                }

                _dbContext.Items.Remove(foundEntity);
                _logger.LogInformation("item with ID {Id} successfully marked for removal in memory context. Awaiting commit.", id);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while executing item destruction sequence for ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemModel>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating database schema scan to compile global items registry report.");

                var entities = await _dbContext.Items
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var itemModelsList = entities.Select(entity => entity.ToModel());

                _logger.LogInformation("Global database scan finalized successfully. Total items loaded into context: {Count}", entities.Count);

                return itemModelsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical data layer disruption encountered while compiling the complete list of items.");
                throw;
            }
        }
    }
}
