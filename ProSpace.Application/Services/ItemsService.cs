using FluentValidation;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Mappers;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;
using System.Data;

namespace ProSpace.Application.Services
{
    public class ItemsService : IItemsService
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<ItemsService> _logger;

        /// <summary>
        /// Item validation service
        /// </summary>
        private readonly IValidator<ItemDto> _validation;

        /// <summary>
        /// Unit of Work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="validation"></param>
        /// <param name="unitOfWork"></param>
        public ItemsService(ILogger<ItemsService> logger, IValidator<ItemDto> validation, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _validation = validation;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> CreateAsync(ItemDto item, CancellationToken cancellationToken = default)
        {
            try
            {
                var validate = await _validation.ValidateAsync(item, cancellationToken);

                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage);
                    _logger.LogInformation("Validation error when creating an item: {Errors}", errors);
                    return BaseIdResponse.Failure(errors);
                }

                var domainItem = item.ToModel();

                await _unitOfWork.ItemsRepository.CreateAsync(domainItem, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Failed to save item {Name} to database during UnitOfWork complete.", item.Name);
                    return BaseIdResponse.Failure("Failed to save the item to the database.");
                }

                _logger.LogInformation("Item {Name} has been successfully created. ItemId: {ItemId}",
                    domainItem.Name, domainItem.Id);

                return BaseIdResponse.Success(domainItem.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error while creating item with Name: {Name}", item.Name);

                return BaseIdResponse.Failure("An internal server error occurred while creating the item.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = await _unitOfWork.ItemsRepository.ReadAsync(id, cancellationToken);

                if (item == null)
                {
                    _logger.LogWarning("Item with ID {Id} not found for deletion.", id);
                    return BaseIdResponse.Failure($"Item with ID {id} not found.");
                }

                await _unitOfWork.ItemsRepository.DeleteAsync(id, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Failed to commit item {Id} deletion to database during UnitOfWork complete.", id);
                    return BaseIdResponse.Failure("Failed to complete item deletion in database.");
                }

                _logger.LogInformation("Item with ID {Id} has been completely deleted from the database", id);
                return BaseIdResponse.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during complete deletion of item {Id}", id);
                return BaseIdResponse.Failure("Failed to delete item");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<IEnumerable<ItemDto>>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var domainItems = await _unitOfWork.ItemsRepository.ReadAllAsync(cancellationToken);

                if (domainItems == null || !domainItems.Any())
                {
                    _logger.LogInformation("No items found in the database.");
                    return BaseResponse<IEnumerable <ItemDto>>.Success([]);
                }

                _logger.LogInformation("Total items found: {Count}", domainItems.Count());

                var dtos = domainItems.Select(x => x.ToDto()).ToArray();

                return BaseResponse<IEnumerable<ItemDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading list of all items.");
                return BaseResponse<IEnumerable<ItemDto>>.Failure("Failed to load item list.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<ItemDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = await _unitOfWork.ItemsRepository.ReadAsync(id, cancellationToken);

                if (item == null)
                {
                    _logger.LogWarning("Item with ID {Id} not found.", id);

                    return BaseResponse<ItemDto>.Failure($"Item with ID {id} not found.");
                }

                var itemDto = item.ToDto();

                _logger.LogInformation("Item loaded successfully: {ItemName}", item.Name);

                return BaseResponse<ItemDto>.Success(itemDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for item by Id: {Id}", id);
                return BaseResponse<ItemDto>.Failure("Unable to search for item due to an internal error.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<ItemDto>> UpdateAsync(ItemDto item, CancellationToken cancellationToken = default)
        {
            try
            {
                var validate = await _validation.ValidateAsync(item, cancellationToken);

                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage);
                    _logger.LogInformation("Validation error when updating item {Id}: {Errors}", item.Id, errors);
                    return BaseResponse<ItemDto>.Failure(errors);
                }

                var existingItem = await _unitOfWork.ItemsRepository.ReadAsync(item.Id, cancellationToken);

                if (existingItem == null)
                {
                    _logger.LogWarning("Failed to update item. Item with ID {Id} not found.", item.Id);
                    return BaseResponse<ItemDto>.Failure($"Item with ID {item.Id} not found.");
                }

                existingItem.Name = item.Name;
                existingItem.Price = item.Price;
                existingItem.Code = item.Code;
                existingItem.Category = item.Category;

                 await _unitOfWork.ItemsRepository.UpdateAsync(existingItem, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogInformation("No database changes required for item {ItemName} (data is identical).", item.Name);
                }

                _logger.LogInformation("Item updated successfully: {ItemName}", existingItem.Name);

                var updatedDto = existingItem.ToDto();

                return BaseResponse<ItemDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item Id: {Id}", item.Id);
                return BaseResponse<ItemDto>.Failure("Failed to update item due to an internal error.");
            }
        }
    }
}
