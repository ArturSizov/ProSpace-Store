using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProSpace.Infrastructure.Mappers;
using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Domain.Models;

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
        public async Task<(CustomerModel?, IDictionary<string, string[]>?)> CreateAsync(CustomerModel entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = entity.ToEntity();
                var result = await _dbContext.Customers.AddAsync(customer, cancellationToken);
                var saved = await _dbContext.SaveChangesAsync(cancellationToken);

                if (saved > 0)
                    return (result.Entity.ToModel(), null);

                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot create an customer");
                return (null, null);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Customers.FindAsync([id], cancellationToken);
                if (found == null)
                {
                    _logger.LogWarning("Cannot find customer with id = {id}", id);
                    return false;
                }

                _ = _dbContext.Customers.Remove(found);

                var saved = await _dbContext.SaveChangesAsync(cancellationToken);

                return saved > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot delete customer");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<CustomerModel?> GetByEmailAsync(string email)
        {
            try
            {
                var customerEntity = await _dbContext.Customers.FirstOrDefaultAsync(c => c.AppUser.Email == email);

                return customerEntity?.ToModel() ?? throw new Exception("Customer not found");
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<CustomerModel[]?> GetByFilterAsync(string code, string name, decimal price, string category, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<CustomerModel[]?> GetByPageAsync(int page, int pasgeSize)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<CustomerModel[]?> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var customers = await _dbContext.Customers.ToArrayAsync();
                return customers.Select(x => x.ToModel()).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot read all customers");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<CustomerModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Customers.FindAsync([id], cancellationToken);
                if (found == null)
                {
                    _logger.LogWarning("Cannot find customer with id = {id}", id);
                    return null;
                }

                return found.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot find customer");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<CustomerModel?> UpdateAsync(CustomerModel entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var found = await _dbContext.Customers.FindAsync([entity.Id], cancellationToken);
                if (found == null)
                {
                    _logger.LogWarning("Cannot find customer with id = {id}", entity.Id);
                    return null;
                }

                found.Name = entity.Name;
                found.Code = entity.Code;
                found.Address = entity.Address;
                found.Discount = entity.Discount;

                _ = _dbContext.Customers.Update(found);

                var saved = await _dbContext.SaveChangesAsync(cancellationToken);
                if (saved > 0)
                    return found.ToModel();

                _logger.LogError("Cannot save the updated data");
                return null;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot update customer");
                return null;
            }
        }
    }
}
