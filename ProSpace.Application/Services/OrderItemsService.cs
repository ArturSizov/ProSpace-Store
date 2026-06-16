using FluentValidation;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Mappers;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Application.Services
{
    public class OrderItemsService : IOrderItemsService
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<OrderItemsService> _logger;

        /// <summary>
        /// Item validation service
        /// </summary>
        private readonly IValidator<CreateOrderItemDto> _validation;

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
        public OrderItemsService(ILogger<OrderItemsService> logger, IValidator<CreateOrderItemDto> validation, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _validation = validation;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> CreateAsync(CreateOrderItemDto orderItem, CancellationToken cancellationToken = default)
        {
            try
            {
                var validate = await _validation.ValidateAsync(orderItem, cancellationToken);

                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage);
                    _logger.LogInformation("Validation error when creating an order item: {Errors}", errors);
                    return BaseIdResponse.Failure(errors);
                }

                var domainItem = orderItem.ToDomainEntity();

                await _unitOfWork.OrderItemsRepository.CreateAsync(domainItem, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Failed to save order item to database during UnitOfWork complete. Parent Order ID: {OrderId}", orderItem.OrderId);
                    return BaseIdResponse.Failure("Failed to save the order item to the database.");
                }

                _logger.LogInformation("Order item with ID {Id} has been successfully created.", domainItem.Id);

                return BaseIdResponse.Success(domainItem.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error while creating order item with ID: {Id}", orderItem.OrderId);

                return BaseIdResponse.Failure("An internal server error occurred while creating the order item.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = await _unitOfWork.OrderItemsRepository.ReadAsync(id, cancellationToken);

                if (item == null)
                {
                    _logger.LogWarning("Order item with ID {Id} not found for deletion.", id);
                    return BaseIdResponse.Failure($"Order item with ID {id} not found.");
                }

                await _unitOfWork.OrderItemsRepository.DeleteAsync(id, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Failed to commit order item {Id} deletion to database during UnitOfWork complete.", id);
                    return BaseIdResponse.Failure("Failed to complete order item deletion in database.");
                }

                _logger.LogInformation("Order item with ID {Id} has been completely deleted from the database", id);
                return BaseIdResponse.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during complete deletion of order item {Id}", id);
                return BaseIdResponse.Failure("Failed to delete order item");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderItemDto[]>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var domainOrderItems = await _unitOfWork.OrderItemsRepository.ReadAllAsync(cancellationToken);

                if (domainOrderItems == null || domainOrderItems.Length == 0)
                {
                    _logger.LogInformation("No order items found in the database.");
                    return BaseResponse<OrderItemDto[]>.Success([]);
                }

                _logger.LogInformation("Total order items found: {Count}", domainOrderItems.Length);

                var dtos = domainOrderItems.Select(x => x.ToDto()).ToArray();

                return BaseResponse<OrderItemDto[]>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading list of all order items.");
                return BaseResponse<OrderItemDto[]>.Failure("Failed to load order item list.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderItemDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var orderItem = await _unitOfWork.OrderItemsRepository.ReadAsync(id, cancellationToken);

                if (orderItem == null)
                {
                    _logger.LogWarning("Order item with ID {Id} not found.", id);

                    return BaseResponse<OrderItemDto>.Failure($"Order item with ID {id} not found.");
                }

                var orderItemDto = orderItem.ToDto();

                _logger.LogInformation("Order item loaded successfully: {Id}", orderItemDto.Id);

                return BaseResponse<OrderItemDto>.Success(orderItemDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for order item by Id: {Id}", id);
                return BaseResponse<OrderItemDto>.Failure("Unable to search for order item due to an internal error.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderItemDto>> UpdateAsync(OrderItemDto orderItem, CancellationToken cancellationToken = default)
        {
            try
            {
                var validate = await _validation.ValidateAsync(orderItem.ToCreateItemDto(), cancellationToken);

                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage);
                    _logger.LogInformation("Validation error when updating order item {Id}: {Errors}", orderItem.Id, errors);
                    return BaseResponse<OrderItemDto>.Failure(errors);
                }

                var existingOrderItem = await _unitOfWork.OrderItemsRepository.ReadAsync(orderItem.Id, cancellationToken);

                if (existingOrderItem == null)
                {
                    _logger.LogWarning("Failed to update order item. Order item with ID {Id} not found.", orderItem.Id);
                    return BaseResponse<OrderItemDto>.Failure($"Orde item with ID {orderItem.Id} not found.");
                }

                existingOrderItem.ItemId = orderItem.ItemId;
                existingOrderItem.OrderId = orderItem.OrderId;
                existingOrderItem.ItemPrice = orderItem.ItemPrice;
                existingOrderItem.ItemsCount = orderItem.ItemsCount;

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                    _logger.LogWarning("No changes were saved for order item {Id} (data might be identical).", orderItem.Id);


                _logger.LogInformation("Order item updated successfully: {Id}", existingOrderItem.Id);

                var updatedDto = existingOrderItem.ToDto();
                return BaseResponse<OrderItemDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order item Id: {Id}", orderItem.Id);
                return BaseResponse<OrderItemDto>.Failure("Failed to update order item due to an internal error.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderItemDto[]>> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var domainOrderItems = await _unitOfWork.OrderItemsRepository.GetOrderItemsByOrderIdAsync(orderId, cancellationToken);

                if (domainOrderItems == null || domainOrderItems.Length == 0)
                {
                    _logger.LogInformation("No order items found in the database for Order ID: {OrderId}", orderId);
                    return BaseResponse<OrderItemDto[]>.Success([]);
                }

                _logger.LogInformation("Total order items found for Order ID {OrderId}: {Count}", orderId, domainOrderItems.Length);

                var dtos = domainOrderItems.Select(x => x.ToDto()).ToArray();

                return BaseResponse<OrderItemDto[]>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading order items list for Order ID: {OrderId}", orderId);
                return BaseResponse<OrderItemDto[]>.Failure("Failed to load order items for the specified order.");
            }
        }

    }
}
