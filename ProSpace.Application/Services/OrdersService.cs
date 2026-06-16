using FluentValidation;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Mappers;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Application.Services
{
    public class OrdersService : IOrderService
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<OrdersService> _logger;

        /// <summary>
        /// Item validation service
        /// </summary>
        private readonly IValidator<CreateOrderDto> _validation;

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
        public OrdersService(ILogger<OrdersService> logger, IValidator<CreateOrderDto> validation, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _validation = validation;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> CreateAsync(CreateOrderDto order, CancellationToken cancellationToken = default)
        {
            try
            {
                var validate = await _validation.ValidateAsync(order, cancellationToken);

                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage);
                    _logger.LogInformation("Validation error when creating an order: {Errors}", errors);
                    return BaseIdResponse.Failure(errors);
                }

                var domainOrder = order.ToDomainEntity();

                await _unitOfWork.OrdersRepository.CreateAsync(domainOrder, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Failed to save order to database during UnitOfWork complete. Order ID: {Id}", domainOrder.Id);
                    return BaseIdResponse.Failure("Failed to save the order to the database.");
                }

                _logger.LogInformation("Order with ID {Id} has been successfully created.", domainOrder.Id);

                return BaseIdResponse.Success(domainOrder.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error while creating order with ID: {Id}", order.OrderNumber);
                return BaseIdResponse.Failure("An internal server error occurred while creating the order.");
            }
        }


        /// <inheritdoc/>
        public async Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = await _unitOfWork.OrdersRepository.ReadAsync(id, cancellationToken);

                if (item == null)
                {
                    _logger.LogWarning("Order with ID {Id} not found for deletion.", id);
                    return BaseIdResponse.Failure($"Order with ID {id} not found.");
                }

                await _unitOfWork.OrdersRepository.DeleteAsync(id, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Failed to commit order {Id} deletion to database during UnitOfWork complete.", id);
                    return BaseIdResponse.Failure("Failed to complete order deletion in database.");
                }

                _logger.LogInformation("Order with ID {Id} has been completely deleted from the database", id);
                return BaseIdResponse.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during complete deletion of order {Id}", id);
                return BaseIdResponse.Failure("Failed to delete order");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderDto[]>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var domainOrders = await _unitOfWork.OrdersRepository.ReadAllAsync(cancellationToken);

                if (domainOrders == null || domainOrders.Length == 0)
                {
                    _logger.LogInformation("No orders found in the database.");
                    return BaseResponse<OrderDto[]>.Success([]);
                }

                _logger.LogInformation("Total orders found: {Count}", domainOrders.Length);

                var dtos = domainOrders.Select(x => x.ToDto()).ToArray();

                return BaseResponse<OrderDto[]>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading list of all orders.");
                return BaseResponse<OrderDto[]>.Failure("Failed to load order list.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _unitOfWork.OrdersRepository.ReadAsync(id, cancellationToken);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {Id} not found.", id);

                    return BaseResponse<OrderDto>.Failure($"Order with ID {id} not found.");
                }

                var orderDto = order.ToDto();

                _logger.LogInformation("Order loaded successfully: {Id}", orderDto.Id);

                return BaseResponse<OrderDto>.Success(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for order by Id: {Id}", id);
                return BaseResponse<OrderDto>.Failure("Unable to search for order due to an internal error.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderDto>> UpdateAsync(OrderDto order, CancellationToken cancellationToken = default)
        {
            try
            {
                var validate = await _validation.ValidateAsync(order.ToCreateItemDto(), cancellationToken);

                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage);
                    _logger.LogInformation("Validation error when updating order {Id}: {Errors}", order.Id, errors);
                    return BaseResponse<OrderDto>.Failure(errors);
                }

                var existingOrder = await _unitOfWork.OrdersRepository.ReadAsync(order.Id, cancellationToken);

                if (existingOrder == null)
                {
                    _logger.LogWarning("Failed to update order. Order with ID {Id} not found.", order.Id);
                    return BaseResponse<OrderDto>.Failure($"Order with ID {order.Id} not found.");
                }

                existingOrder.OrderNumber = order.OrderNumber;
                existingOrder.OrderDate = order.OrderDate;
                existingOrder.ShipmentDate = order.ShipmentDate;
                existingOrder.Status = order.Status;
                existingOrder.CustomerId = order.CustomerId;

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                    _logger.LogWarning("No changes were saved for order item {Id} (data might be identical).", order.Id);


                _logger.LogInformation("Order item updated successfully: {Id}", existingOrder.Id);

                var updatedDto = existingOrder.ToDto();
                return BaseResponse<OrderDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order Id: {Id}", order.Id);
                return BaseResponse<OrderDto>.Failure("Failed to update order due to an internal error.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderDto[]>> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default)
        {
            try
            {
                var domainOrders = await _unitOfWork.OrdersRepository.GetByCustomerCodeAsync(customerCode, cancellationToken);

                if (domainOrders == null || domainOrders.Length == 0)
                {
                    _logger.LogInformation("No orders found in the database for customer code: {customerCode}", customerCode);
                    return BaseResponse<OrderDto[]>.Success([]);
                }

                _logger.LogInformation("Total orders found for customer code: {customerCode}: {Count}", customerCode, domainOrders.Length);

                var dtos = domainOrders.Select(x => x.ToDto()).ToArray();

                return BaseResponse<OrderDto[]>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading orders list for customer code: {CustomerCode}", customerCode);
                return BaseResponse<OrderDto[]>.Failure("Failed to load orders for the specified customer.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderDto[]>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var domainOrders = await _unitOfWork.OrdersRepository.GetByCustomerIdAsync(customerId, cancellationToken);

                if (domainOrders == null || domainOrders.Length == 0)
                {
                    _logger.LogInformation("No orders found in the database for customer ID: {customerId}", customerId);
                    return BaseResponse<OrderDto[]>.Success([]);
                }

                _logger.LogInformation("Total orders found for customer ID: {customerId}: {Count}", customerId, domainOrders.Length);

                var dtos = domainOrders.Select(x => x.ToDto()).ToArray();

                return BaseResponse<OrderDto[]>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading orders list for customer ID: {CustomerId}", customerId);
                return BaseResponse<OrderDto[]>.Failure("Failed to load orders for the specified customer.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderDto>> GetByOrderNumberAsync(int orderNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _unitOfWork.OrdersRepository.GetByOrderNumberAsync(orderNumber, cancellationToken);

                if (order == null)
                {
                    _logger.LogWarning("Order with number {orderNumber} not found.", orderNumber);

                    return BaseResponse<OrderDto>.Failure($"Order with number {orderNumber} not found.");
                }

                var orderDto = order.ToDto();

                _logger.LogInformation("Order loaded successfully: {OrderNumber}", orderDto.OrderNumber);

                return BaseResponse<OrderDto>.Success(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for order by number: {Number}", orderNumber);
                return BaseResponse<OrderDto>.Failure("Unable to search for order due to an internal error.");
            }
        }
    }
}
