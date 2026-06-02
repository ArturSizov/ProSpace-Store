using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Domain.Interfaces.Services;
using ProSpace.Domain.Interfaces.Validations;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Services
{
    public class OrdersService : IOrderService
    {
        /// <summary>
        /// Validation service
        /// </summary>
        private readonly IValidationProvider<OrderModel> _validation;

        /// <summary>
        /// Unit of Work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="unitOfWork"></param>
        public OrdersService(IValidationProvider<OrderModel> validation, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _validation = validation;
        }

        /// <inheritdoc/>
        public async Task<(OrderModel?, IDictionary<string, string[]>?)> CreateAsync(OrderModel order, CancellationToken cancellationToken = default)
        {
            var isValid = await _validation.ValidateAsync(order);

            if (!isValid.Item1)
                return (null, isValid.Item2);

            var result = await _unitOfWork.OrdersRepository.CreateAsync(order, cancellationToken);

            return (result.Item1, null);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => _unitOfWork.OrdersRepository.DeleteAsync(id, cancellationToken);

        /// <inheritdoc/>
        public Task<OrderModel[]?> GetByCustomerCodeAsync(string customerCode)
            => _unitOfWork.OrdersRepository.GetByCustomerCode(customerCode);

        /// <inheritdoc/>
        public Task<OrderModel[]?> GetByCustomerId(Guid customerId)
            => _unitOfWork.OrdersRepository.GetByCustomerId(customerId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<OrderModel?> GetByOrderNumber(int orderNumber)
            => _unitOfWork.OrdersRepository.GetByOrderNumber(orderNumber);

        /// <inheritdoc/>
        public Task<OrderModel[]?> ReadAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.OrdersRepository.ReadAllAsync(cancellationToken);

        /// <inheritdoc/>
        public Task<OrderModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
            => _unitOfWork.OrdersRepository.ReadAsync(id, cancellationToken);

        /// <inheritdoc/>
        public async Task<OrderModel?> UpdateAsync(OrderModel order, CancellationToken cancellationToken = default)
        {
            var isValid = await _validation.ValidateAsync(order);

            if (!isValid.Item1)
                return null;

            return await _unitOfWork.OrdersRepository.UpdateAsync(order, cancellationToken);
        }
    }
}
