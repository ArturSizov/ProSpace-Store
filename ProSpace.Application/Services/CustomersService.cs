using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Domain.Interfaces.Services;
using ProSpace.Domain.Interfaces.Validations;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Services
{
    public class CustomersService : ICustomersService
    {
        /// <summary>
        /// Validation service
        /// </summary>
        private readonly IValidationProvider<CustomerModel> _validation;

        /// <summary>
        /// Unit of Work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="unitOfWork"></param>
        public CustomersService(IValidationProvider<CustomerModel> validation, IUnitOfWork unitOfWork)
        {
            _validation = validation;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<(CustomerModel?, IDictionary<string, string[]>?)> CreateAsync(CustomerModel customer, CancellationToken cancellationToken = default)
        {
            var isValid = await _validation.ValidateAsync(customer);

            if (!isValid.Item1)
                return (null, isValid.Item2);

            var result = await _unitOfWork.CustomersRepository.CreateAsync(customer, cancellationToken);

            return (result.Item1, null);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => _unitOfWork.CustomersRepository.DeleteAsync(id, cancellationToken);

        public Task<CustomerModel?> GetByEmailAsync(string email)
            => _unitOfWork.CustomersRepository.GetByEmailAsync(email);


        /// <inheritdoc/>
        public Task<CustomerModel[]?> ReadAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.CustomersRepository.ReadAllAsync(cancellationToken);

        /// <inheritdoc/>
        public Task<CustomerModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
            => _unitOfWork.CustomersRepository.ReadAsync(id, cancellationToken);

        /// <inheritdoc/>
        public async Task<CustomerModel?> UpdateAsync(CustomerModel customer, CancellationToken cancellationToken = default)
        {
            var isValid = await _validation.ValidateAsync(customer);

            if (!isValid.Item1)
                return null;

            return await _unitOfWork.CustomersRepository.UpdateAsync(customer, cancellationToken);
        }
    }
}
