using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Domain.Interfaces.Services;
using ProSpace.Domain.Interfaces.Validations;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Services
{
    public class ItemsService : IItemsService
    {
        /// <summary>
        /// Validation service
        /// </summary>
        private readonly IValidationProvider<ItemModel> _validation;

        /// <summary>
        /// Unit of Work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="unitOfWork"></param>
        public ItemsService(IValidationProvider<ItemModel> validation, IUnitOfWork unitOfWork)
        {
            _validation = validation;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public Task<ItemModel[]?> ReadAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.ItemsRepository.ReadAllAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task<(ItemModel?, IDictionary<string, string[]>?)> CreateAsync(ItemModel item, CancellationToken cancellationToken = default)
        {
            var isValid = await _validation.ValidateAsync(item);

            if (!isValid.Item1)
                return (null, isValid.Item2);

            var result = await _unitOfWork.ItemsRepository.CreateAsync(item, cancellationToken);

            return (result.Item1, null);
        }

        /// <inheritdoc/>
        public Task<ItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
            => _unitOfWork.ItemsRepository.ReadAsync(id, cancellationToken);

        /// <inheritdoc/>
        public async Task<ItemModel?> UpdateAsync(ItemModel model, CancellationToken cancellationToken = default)
        {
            var isValid = await _validation.ValidateAsync(model);

            if (!isValid.Item1)
                return null;

            return await _unitOfWork.ItemsRepository.UpdateAsync(model, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => _unitOfWork.ItemsRepository.DeleteAsync(id, cancellationToken);
    }
}
