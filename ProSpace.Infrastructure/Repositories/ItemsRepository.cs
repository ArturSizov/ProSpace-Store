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
                var entity = item.ToEntity();

                await _dbContext.Items.AddAsync(entity, cancellationToken);

                _logger.LogInformation("Item {Id} attached to the context for creation", item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error adding item {Id} to DB context", item.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Items
                         .AsNoTracking()
                         .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Cannot find items with id = {id}", id);
                    return null;
                }

                return found.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error reading item {Id} in DB context", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(ItemModel item, CancellationToken cancellationToken = default)
        {
            var found = await _dbContext.Items.FindAsync([item.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Item with id = {item.Id} not found.");

            found.Name = item.Name;
            found.Code = item.Code;
            found.Price = item.Price;
            found.Category = item.Category;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Items.FindAsync([id], cancellationToken);

                if (found != null)
                {
                    _dbContext.Items.Remove(found);
                    _logger.LogInformation("Item with ID {CustomerId} marked for deletion", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error deleting item {Id} in DB context: {Message}", id, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemModel[]> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var items = await _dbContext.Items
                          .AsNoTracking()
                          .Select(o => o.ToModel())
                          .ToArrayAsync(cancellationToken);

                _logger.LogInformation("Total items loaded: {Length}", items.Length);

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading list of all items from database");

                throw;
            }
        }
    }
}
