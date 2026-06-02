using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Domain.Interfaces.Services;
using ProSpace.Domain.Interfaces.Validations;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Services
{
    public class OrderItemsService : IOtderItemsService
    {
        /// <summary>
        /// Validation service
        /// </summary>
        private readonly IValidationProvider<OrderItemModel> _validation;

        /// <summary>
        /// Unit of Work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="unitOfWork"></param>
        public OrderItemsService(IValidationProvider<OrderItemModel> validation, IUnitOfWork unitOfWork)
        {
            _validation = validation;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<(OrderItemModel?, IDictionary<string, string[]>?)> CreateAsync(OrderItemModel orderItem, CancellationToken cancellationToken = default)
        {
            var isValid = await _validation.ValidateAsync(orderItem);

            if (!isValid.Item1)
                return (null, isValid.Item2);

            var result = await _unitOfWork.OrderItemsRepository.CreateAsync(orderItem, cancellationToken);

            return (result.Item1, null);
        }

        /// <inheritdoc/>
        public Task<OrderItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
            => _unitOfWork.OrderItemsRepository.ReadAsync(id, cancellationToken);

        /// <inheritdoc/>
        public async Task<OrderItemModel?> UpdateAsync(OrderItemModel orderItem, CancellationToken cancellationToken = default)
        {
            var isValid = await _validation.ValidateAsync(orderItem);

            if (!isValid.Item1)
                return null;

            return await _unitOfWork.OrderItemsRepository.UpdateAsync(orderItem, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => _unitOfWork.OrderItemsRepository.DeleteAsync(id, cancellationToken);

        /// <inheritdoc/>
        public Task<OrderItemModel[]?> ReadAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.OrderItemsRepository.ReadAllAsync(cancellationToken);

        /// <inheritdoc/>
        public Task<OrderItemModel[]?> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
            => _unitOfWork.OrderItemsRepository.GetOrderItemsByOrderIdAsync(orderId, cancellationToken);
    }
}
