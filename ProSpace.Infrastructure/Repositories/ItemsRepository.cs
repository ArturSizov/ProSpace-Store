using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProSpace.Infrastructure.Mappers;
using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Domain.Models;
using ProSpace.Infrastructure.Entites.Supply;

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
        public async Task<(ItemModel?, IDictionary<string, string[]>?)> CreateAsync(ItemModel entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = entity.ToEntity();
                var result = await _dbContext.Items.AddAsync(item, cancellationToken);
                var saved = await _dbContext.SaveChangesAsync(cancellationToken);

                if (saved > 0)
                    return (result.Entity.ToModel(), null);

                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot create an item");
                return (null, null);
            }
        }

        /// <inheritdoc/>
        public async Task<ItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Items.FindAsync([id], cancellationToken);
                if (found == null)
                {
                    _logger.LogWarning("Cannot find item with id = {id}", id);
                    return null;
                }

                return found.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot find item");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemModel?> UpdateAsync(ItemModel entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Items.FindAsync([entity.Id], cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Cannot find item with id = {id}", entity.Id);
                    return null;
                }

                found.Name = entity.Name;
                found.Code = entity.Code;
                found.Price = entity.Price;
                found.Category = entity.Category;

                _ = _dbContext.Items.Update(found);

                var saved = await _dbContext.SaveChangesAsync(cancellationToken);

                if (saved > 0)
                    return found.ToModel();

                _logger.LogError("Cannot save the updated data");
                return null;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot update item");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Items.FindAsync([id], cancellationToken);
                if (found == null)
                {
                    _logger.LogWarning("Cannot find item with id = {id}", id);
                    return false;
                }

                _ = _dbContext.Items.Remove(found);

                var saved = await _dbContext.SaveChangesAsync(cancellationToken);

                return saved > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot delete item");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemModel[]?> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var items = await _dbContext.Items.ToArrayAsync();
                return items.Select(x => x.ToModel()).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot read all items");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemModel[]?> GetByFilterAsync(string code, string name, decimal price, string category, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Items.AsNoTracking();

            if(!string.IsNullOrEmpty(code))
                query = query.Where(i => i.Code.Contains(code));

            if (!string.IsNullOrEmpty(name))
                query = query.Where(i => i.Name.Contains(name));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(i => i.Category!.Contains(category));

            if (price > 0)
                query = query.Where(i => i.Price > price);

            var items = await query.ToListAsync();

            return items.Select(x => x.ToModel()).ToArray();
        }

        /// <inheritdoc/>
        public async Task<ItemModel[]?> GetByPageAsync(int page, int pasgeSize)
        {
            var query = await _dbContext.Items.AsNoTracking()
                                               .Skip((page - 1) * pasgeSize)
                                               .Take(pasgeSize)
                                               .ToListAsync();

            return query.Select(x => x.ToModel()).ToArray();
        }
    }
}
