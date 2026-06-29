using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Domain.Models;
using ProSpace.Infrastructure.Entites.Supply;
using ProSpace.Infrastructure.Mappers;
using System.Linq.Expressions;

namespace ProSpace.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomersRepository
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
        public CustomerRepository(ILogger<CustomerRepository> logger, ProSpaceDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(CustomerModel customer, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating database insertion sequence for a new Customer profile entity record: {Id}", customer.Id);

                var entity = customer.ToEntity();

                await _dbContext.Customers.AddAsync(entity, cancellationToken);

                _logger.LogInformation("Customer profile record {Id} successfully attached to the database memory context. Awaiting transaction commit.", customer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while adding a new Customer to DB context for ID: {Id}", customer.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to locate and extract Customer profile for permanent removal: {Id}", id);

                var foundEntity = await _dbContext.Customers
                    .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogWarning("Extraction operation aborted. Customer profile with identity tracking token {Id} was not located.", id);
                    throw new KeyNotFoundException($"Customer with id = {id} was not found in the persistent storage context.");
                }

                _dbContext.Customers.Remove(foundEntity);

                _logger.LogInformation("Customer profile with ID {Id} successfully marked for removal in memory context. Awaiting transaction commit.", id);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while executing customer profile destruction sequence for ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CustomerModel>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating database schema scan to compile global customers registry report.");

                var entities = await _dbContext.Customers
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var customerModelsList = entities.Select(entity => entity.ToModel());

                _logger.LogInformation("Global database scan finalized successfully. Total customers loaded into context: {Count}", entities.Count);

                return customerModelsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical data layer disruption encountered while compiling the complete list of customers.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CustomerModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Scanning database schema for single customer record matching tracking identity: {Id}", id);

                var foundEntity = await _dbContext.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogInformation("No single customer record matches the requested database tracking identifier: {Id}", id);
                    return null;
                }

                return foundEntity.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while reading single customer for tracking identity: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(CustomerModel customer, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to locate and update persistent state parameters for Customer profile: {Id}", customer.Id);

                var foundEntity = await _dbContext.Customers.FindAsync([customer.Id], cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogWarning("Data modification aborted. Customer profile with tracking ID {Id} was not found inside the database registers.", customer.Id);
                    throw new KeyNotFoundException($"Customer with id = {customer.Id} not found in the persistent storage context.");
                }

                foundEntity.Name = customer.Name;
                foundEntity.Code = customer.Code;
                foundEntity.Address = customer.Address;
                foundEntity.Discount = customer.Discount;

                _dbContext.Customers.Update(foundEntity);

                _logger.LogInformation("Customer profile {Id} parameters successfully synchronized in-memory. Awaiting transaction commit.", customer.Id);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while updating state parameters for Customer profile: {Id}", customer.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CustomerModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating multi-tier identity scan loop to locate customer profile for email: {Email}", email);

                var foundEntity = await _dbContext.Customers
                    .FirstOrDefaultAsync(customer => _dbContext.Users.Any(user => user.Id == customer.AppUserId && user.Email == email), cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogInformation("No single customer record matches the requested authorization identity email credentials: {Email}", email);
                    return null;
                }

                return foundEntity.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while reading customer by email identity: {Email}", email);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CustomerModel?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Scanning database schema for customer record linked to Identity User ID: {AppUserId}", appUserId);

                var foundEntity = await _dbContext.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(customer => customer.AppUserId == appUserId, cancellationToken);

                if (foundEntity == null)
                {
                    _logger.LogInformation("No single customer record matches the requested authorization identity app user ID credentials: {AppUserId}", appUserId);
                    return null;
                }

                return foundEntity.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database layer disruption encountered while reading customer by app user ID identity: {AppUserId}", appUserId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _dbContext.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (entity != null)
            {
                entity.IsDeleted = true;

                _dbContext.Customers.Update(entity);
            }
        }

        /// <inheritdoc/>
        public async Task<string> GenerateNextCustomerCodeAsync(CancellationToken ct)
        {
            var allCodes = await _dbContext.Customers
                .Select(c => c.Code)
                .ToListAsync(ct);

            int maxCodeNumber = allCodes
                .Select(code => code?.Replace("-", "") ?? string.Empty)
                .Select(code => int.TryParse(code, out var num) ? num : 0)
                .DefaultIfEmpty(0)
                .Max();

            int nextNumber = maxCodeNumber + 1;

            return nextNumber.ToString("D8").Insert(4, "-");
        }

        /// <inheritdoc/>
        public async Task<bool> IsCodeAssignedToAnotherCustomerAsync(string code, Guid excludingCustomerId, CancellationToken ct)
            => await _dbContext.Customers.AnyAsync(c => c.Code == code && c.Id != excludingCustomerId, ct);
    }
}
