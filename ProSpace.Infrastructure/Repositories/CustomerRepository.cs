using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Domain.Models;
using ProSpace.Infrastructure.Mappers;

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
                var entity = customer.ToEntity();

                await _dbContext.Customers.AddAsync(entity, cancellationToken);

                _logger.LogInformation("Customers {Id} attached to the context for creation", customer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error adding customer {Id} to DB context", customer.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Customers
                         .AsNoTracking()
                         .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Delete failed. Customer with ID {Id} not found", id);
                    throw new KeyNotFoundException($"Customer with id = {id} was not found.");
                }

                _dbContext.Customers.Remove(found);
                _logger.LogInformation("Customer with ID {Id} marked for deletion in memory context", id);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Critical error deleting customer {Id} in DB context", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CustomerModel[]> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var customers = await _dbContext.Customers
                          .AsNoTracking()
                          .Select(o => o.ToModel())
                          .ToArrayAsync(cancellationToken);

                _logger.LogInformation("Total customers loaded: {Length}", customers.Length);

                return customers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading list of all customers from database");

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CustomerModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Customers
                         .AsNoTracking()
                         .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Cannot find customer with id = {id}", id);
                    return null;
                }

                return found.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error reading customer {Id} in DB context", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(CustomerModel customer, CancellationToken cancellationToken = default)
        {
            var found = await _dbContext.Customers.FindAsync([customer.Id], cancellationToken)
                 ?? throw new KeyNotFoundException($"Customer with id = {customer.Id} not found.");

            found.Name = customer.Name;
            found.Code = customer.Code;
            found.Address = customer.Address;
            found.Discount = customer.Discount;
        }

        /// <inheritdoc/>
        public async Task<CustomerModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.AppUser.Email == email, cancellationToken);

                if (found == null)
                {
                    _logger.LogWarning("Cannot find customers with email = {email}", email);
                    return null;
                }

                return found.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error reading customer {Email} in DB context", email);
                throw;
            }
        }

    }
}
