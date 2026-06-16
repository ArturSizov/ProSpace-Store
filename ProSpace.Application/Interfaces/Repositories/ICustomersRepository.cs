using ProSpace.Domain.Models;

namespace ProSpace.Application.Interfaces.Repositories
{
    public interface ICustomersRepository : IBasicCRUD<CustomerModel, Guid>
    {
        /// <summary>
        /// Returns the customer by code 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<CustomerModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
