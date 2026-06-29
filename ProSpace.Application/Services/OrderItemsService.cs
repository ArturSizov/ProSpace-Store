using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Mappers;
using ProSpace.Contracts.Contracts.Request.OrderItem;
using ProSpace.Contracts.DTO.OrderItem;
using ProSpace.Contracts.Responses;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Services
{
    public class OrderItemsService : IOrderItemsService
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<OrderItemsService> _logger;

        /// <summary>
        /// Create Order item validation service
        /// </summary>
        private readonly IValidator<CreateOrderItemDto> _createOrderItemValidation;

        /// <summary>
        /// Update Order item validation service
        /// </summary>
        private readonly IValidator<UpdateOrderItemDto> _updateOrderItemValidation;

        /// <summary>
        /// Unit of Work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Security service
        /// </summary>
        private readonly ISecurityService _securityService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="createOrderItemValidation"></param>
        /// <param name="updateOrderItemValidation"></param>
        /// <param name="unitOfWork"></param>
        public OrderItemsService(ILogger<OrderItemsService> logger, IValidator<CreateOrderItemDto> createOrderItemValidation, 
            IValidator<UpdateOrderItemDto> updateOrderItemValidation,
            IUnitOfWork unitOfWork, ISecurityService securityService)
        {
            _logger = logger;
            _createOrderItemValidation = createOrderItemValidation;
            _updateOrderItemValidation = updateOrderItemValidation;
            _unitOfWork = unitOfWork;
            _securityService = securityService;
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> CreateAsync(CreateOrderItemDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing order item creation sequence for product {ItemId} inside Order {OrderId}", dto.ItemId, dto.OrderId);

                var validate = await _createOrderItemValidation.ValidateAsync(dto, cancellationToken);

                if (!validate.IsValid)
                {
                    var validationErrorsCollection = validate.Errors.Select(e => e.ErrorMessage);

                    _logger.LogWarning("Request to create exceptions at the next processing stage. Input data properties failed validation against the rules: {Errors}",
                        string.Join("; ", validationErrorsCollection));

                    return BaseIdResponse.Failure(validationErrorsCollection);
                }

                var catalogItem = await _unitOfWork.ItemsRepository.ReadAsync(dto.ItemId, cancellationToken);
                if (catalogItem == null)
                {
                    _logger.LogWarning("Creation rejected. Product {ItemId} does not exist in the catalog registries.", dto.ItemId);
                    return BaseIdResponse.Failure("The selected product does not exist in our catalog.");
                }

                var parentOrder = await _unitOfWork.OrdersRepository.ReadAsync(dto.OrderId, cancellationToken);
                if (parentOrder == null)
                {
                    _logger.LogWarning("Creation rejected. Parent Order {OrderId} was not located inside persistent stores.", dto.OrderId);
                    return BaseIdResponse.Failure("Target order container was not found.");
                }

                var (IsSuccess, Error) = await _securityService.ValidateCustomerAccessAsync(parentOrder.CustomerId, cancellationToken);

                if (!IsSuccess)
                {
                    _logger.LogCritical("Security breach attempt blocked! Unauthorized user tried to insert item line into Order: {OrderId}", dto.OrderId);
                    return BaseIdResponse.Failure(Error);
                }

                if (parentOrder.Status != "New")
                {
                    _logger.LogWarning("Creation rejected. Order {OrderId} is locked under status: {Status}", dto.OrderId, parentOrder.Status);
                    return BaseIdResponse.Failure("Cannot add items to an active or processed order.");
                }

                var customer = await _unitOfWork.CustomersRepository.ReadAsync(parentOrder.CustomerId, cancellationToken);
                if (customer == null)
                {
                    _logger.LogWarning("Creation rejected. Customer profile linked to order ID {OrderId} not found.", dto.OrderId);
                    return BaseIdResponse.Failure("The customer profile associated with this order container was not found.");
                }

                decimal singleUnitDiscountedPrice = catalogItem.Price * (1m - (customer.Discount / 100m));

                var domainOrderItem = new OrderItemModel
                {
                    Id = Guid.NewGuid(),
                    OrderId = dto.OrderId,
                    ItemId = dto.ItemId,
                    ItemsCount = dto.ItemsCount,
                    ItemPrice = singleUnitDiscountedPrice
                };

                await _unitOfWork.OrderItemsRepository.CreateAsync(domainOrderItem, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Database unit of work flush sequence failed for order item allocation inside Order: {OrderId}", dto.OrderId);
                    return BaseIdResponse.Failure("Failed to add the product item to the order due to database storage exception.");
                }

                _logger.LogInformation("Order item {Id} successfully instantiated for Order {OrderId}. ItemPrice: {Price} (Count: {Count})",
                    domainOrderItem.Id, dto.OrderId, domainOrderItem.ItemPrice, domainOrderItem.ItemsCount);

                return BaseIdResponse.Success(domainOrderItem.Id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical unhandled execution exception crashed order item provision loop for Order: {OrderId}", dto.OrderId);
                return BaseIdResponse.Failure("An internal service error occurred during order item provision processing tasks.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating secure deletion sequence for order item tracking key: {Id}", id);

                var orderItem = await _unitOfWork.OrderItemsRepository.ReadAsync(id, cancellationToken);

                if (orderItem == null)
                {
                    _logger.LogWarning("Extraction operation suspended. Order item with ID {Id} was not located.", id);
                    return BaseIdResponse.Failure($"Order item with ID {id} not found inside system registers.");
                }

                var parentOrder = await _unitOfWork.OrdersRepository.ReadAsync(orderItem.OrderId, cancellationToken);

                if (parentOrder == null)
                {
                    _logger.LogError("Database integrity violation: Order item {Id} points to non-existent Order {OrderId}.",
                        orderItem.Id, orderItem.OrderId);
                    return BaseIdResponse.Failure("Internal reference consistency breakdown. Parent order container missing.");
                }

                var (isSuccess, error) = await _securityService.ValidateCustomerAccessAsync(parentOrder.CustomerId, cancellationToken);

                if (!isSuccess)
                {
                    _logger.LogCritical("Security breach attempt blocked! Unauthorized user tried to delete Order Item {ItemId} from Order {OrderId}",
                        id, parentOrder.Id);
                    return BaseIdResponse.Failure(error);
                }

                if (!_securityService.IsManager() && parentOrder.Status != "New")
                {
                    _logger.LogWarning("Extraction operation rejected. Customer tried to delete Item {ItemId} from active Order {OrderId} with status: {Status}",
                        id, parentOrder.Id, parentOrder.Status);
                    return BaseIdResponse.Failure("Access denied. Items can only be extracted from new unfulfilled orders.");
                }

                await _unitOfWork.OrderItemsRepository.DeleteAsync(id, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Database unit of work flush sequence failed during order item extraction for ID: {Id}", id);
                    return BaseIdResponse.Failure("Failed to complete the atomic order item deletion inside persistent stores.");
                }

                _logger.LogInformation("Order item with ID {Id} has been successfully and completely deleted from the database.", id);

                return BaseIdResponse.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An unhandled execution crash disrupted order item extraction pipelines for ID: {Id}", id);
                return BaseIdResponse.Failure("An internal server exception tracking error occurred during order item extraction operations.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<IEnumerable<OrderItemDto>>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating secure global collection scan for order items data models layer.");

                if (_securityService.IsManager())
                {
                    _logger.LogInformation("Manager privileges verified. Fetching the complete database sequence.");

                    var allOrderItems = await _unitOfWork.OrderItemsRepository.ReadAllAsync(cancellationToken);

                    var allDtos = allOrderItems?.Select(item => item.ToDto()).ToList() ?? Enumerable.Empty<OrderItemDto>();

                    return BaseResponse<IEnumerable<OrderItemDto>>.Success(allDtos);
                }

                var authenticatedCustomerId = await _securityService.GetCurrentCustomerIdAsync(cancellationToken);

                if (authenticatedCustomerId == null || authenticatedCustomerId == Guid.Empty)
                {
                    _logger.LogWarning("Orders compilation terminated. Active user context mapping not found.");

                    return BaseResponse<IEnumerable<OrderItemDto>>.Success([]);
                }

                _logger.LogInformation("Standard customer account verified. Executing isolated database query for Customer ID: {CustomerId}", authenticatedCustomerId);

                var customerSpecificOrderItemsList = await _unitOfWork.OrderItemsRepository.GetOrderItemsByCustomerIdAsync(authenticatedCustomerId.Value, cancellationToken);

                var filteredCustomerOrderItemsDtos = customerSpecificOrderItemsList?.Select(order => order.ToDto()).ToList() ?? [];

                _logger.LogInformation("Successfully compiled safe isolated orders ledger sequence for Customer profile: {CustomerId}. Total loaded: {Count}",
                    authenticatedCustomerId, filteredCustomerOrderItemsDtos.Count);

                return BaseResponse<IEnumerable<OrderItemDto>>.Success(filteredCustomerOrderItemsDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception framework crash disrupted global order items resolution workflows.");
                return BaseResponse<IEnumerable<OrderItemDto>>.Failure("An internal server error occurred.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderItemDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("The process of reading the Order Item by ID has begun.");

                var orderItem = await _unitOfWork.OrderItemsRepository.ReadAsync(id, cancellationToken);

                if (orderItem == null)
                {
                    _logger.LogWarning("Order item with ID {Id} not found.", id);
                    return BaseResponse<OrderItemDto>.Failure($"Order item with ID {id} not found.");
                }

                var parentOrder = await _unitOfWork.OrdersRepository.ReadAsync(orderItem.OrderId, cancellationToken);

                if (parentOrder == null)
                {
                    _logger.LogError("Database integrity violation: Order item {Id} points to non-existent Order {OrderId}.",
                        orderItem.Id, orderItem.OrderId);
                    return BaseResponse<OrderItemDto>.Failure($"Order item with ID {id} not found.");
                }

                var (isSuccess, error) = await _securityService.ValidateCustomerAccessAsync(parentOrder.CustomerId, cancellationToken);

                if (!isSuccess)
                {
                    _logger.LogCritical("Security enforcement blocked unauthorized access attempt to Order Item ID: {Id}", id);
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
        public async Task<BaseResponse<OrderItemDto>> UpdateAsync(UpdateOrderItemDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating update for order item: {Id}", dto.Id);

                var validate = await _updateOrderItemValidation.ValidateAsync(dto, cancellationToken);

                if (!validate.IsValid)
                {
                    var validationErrorsCollection = validate.Errors.Select(e => e.ErrorMessage);

                    _logger.LogWarning("Request to create exceptions at the next processing stage. Input data properties failed validation against the rules: {Errors}",
                        string.Join("; ", validationErrorsCollection));

                    return BaseResponse<OrderItemDto>.Failure(validationErrorsCollection);
                }

                var existingOrderItem = await _unitOfWork.OrderItemsRepository.ReadAsync(dto.Id, cancellationToken);
                if (existingOrderItem == null)
                    return BaseResponse<OrderItemDto>.Failure($"Order item with ID {dto.Id} not found.");

                var parentOrder = await _unitOfWork.OrdersRepository.ReadAsync(existingOrderItem.OrderId, cancellationToken);
                if (parentOrder == null)
                    return BaseResponse<OrderItemDto>.Failure("Internal reference consistency breakdown. Parent order missing.");

                var (isSuccess, error) = await _securityService.ValidateCustomerAccessAsync(parentOrder.CustomerId, cancellationToken);
                if (!isSuccess)
                {
                    _logger.LogCritical("Security breach attempt! Unauthorized item modification blocked for Order Item: {Id}", dto.Id);
                    return BaseResponse<OrderItemDto>.Failure($"Order item with ID {dto.Id} not found.");
                }

                if (!_securityService.IsManager() && parentOrder.Status != "New")
                {
                    _logger.LogWarning("Modification rejected. Customer tried to update Item {ItemId} inside active Order {OrderId} with status {Status}",
                        dto.Id, parentOrder.Id, parentOrder.Status);
                    return BaseResponse<OrderItemDto>.Failure("Access denied. Items can only be modified in new unfulfilled orders.");
                }

                var catalogItem = await _unitOfWork.ItemsRepository.ReadAsync(dto.ItemId, cancellationToken);
                if (catalogItem == null)
                {
                    _logger.LogWarning("Update rejected. Product {ItemId} does not exist in the catalog registries.", dto.ItemId);
                    return BaseResponse<OrderItemDto>.Failure("The selected product does not exist in our catalog.");
                }

                _logger.LogInformation("Calculating dynamic price based on catalog and customer discount for item {Id}", dto.Id);

                var customer = await _unitOfWork.CustomersRepository.ReadAsync(parentOrder.CustomerId, cancellationToken);
                decimal discount = customer?.Discount ?? 0m;
                decimal determinedFinalPrice = catalogItem.Price * (1m - (discount / 100m));

                if (_securityService.IsManager())
                {
                    if (dto.ItemPrice != 0)
                    {
                        _logger.LogInformation("Manager administrative price override applied for item {Id}: {Price}", dto.Id, dto.ItemPrice);
                        determinedFinalPrice = dto.ItemPrice;
                    }
                }
                else
                {
                    if (dto.ItemPrice != 0)
                    {
                        _logger.LogCritical("Security integrity alert! Customer tried to inject a custom price: {Price} for item {Id}", dto.ItemPrice, dto.Id);
                        return BaseResponse<OrderItemDto>.Failure("Access denied. Custom pricing parameters are restricted.");
                    }
                }

                existingOrderItem.ItemsCount = dto.ItemsCount;
                existingOrderItem.ItemId = dto.ItemId;
                existingOrderItem.ItemPrice = determinedFinalPrice;

                await _unitOfWork.OrderItemsRepository.UpdateAsync(existingOrderItem, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                    _logger.LogWarning("No structural data changes were stored for order item {Id} (data payload might be identical).", dto.Id);
                else
                    _logger.LogInformation("Order item updated successfully. ID: {Id}", dto.Id);

                return BaseResponse<OrderItemDto>.Success(existingOrderItem.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical unhandled execution exception crashed order item state synchronization for ID: {Id}", dto.Id);
                return BaseResponse<OrderItemDto>.Failure("An internal service error occurred while processing the order item update transaction.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<IEnumerable<OrderItemDto>>> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing secure data isolation routing layout scans for order items under Order: {OrderId}", orderId);

                if (_securityService.IsManager())
                {
                    var allOrderItemsList = await _unitOfWork.OrderItemsRepository.GetOrderItemsByOrderIdAsync(orderId, cancellationToken);
                    var allOrderItemDtosCollection = allOrderItemsList?.Select(order => order.ToDto()).ToList() ?? [];

                    _logger.LogInformation("Manager query: Total order items found by order ID {Id}: {Count}", orderId, allOrderItemDtosCollection.Count);

                    return BaseResponse<IEnumerable<OrderItemDto>>.Success(allOrderItemDtosCollection);
                }

                var authenticatedCustomerId = await _securityService.GetCurrentCustomerIdAsync(cancellationToken);

                if (authenticatedCustomerId == null || authenticatedCustomerId == Guid.Empty)
                {
                    _logger.LogWarning("Order items compilation terminated. Active user context mapping not found.");
                    return BaseResponse<IEnumerable<OrderItemDto>>.Success([]);
                }

                _logger.LogInformation("Standard customer account verified. Validating requested Order ID: {OrderId}", orderId);

                var orderItemsCollection = await _unitOfWork.OrderItemsRepository.GetOrderItemsByOrderIdAsync(orderId, cancellationToken);

                var orderItemsDtoList = orderItemsCollection?.Select(item => item.ToDto());

                _logger.LogInformation("Successfully compiled lines database traces context array for order group node: {OrderId}.", orderId);

                return BaseResponse<IEnumerable<OrderItemDto>>.Success(orderItemsDtoList ?? []);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception framework crash disrupted line items gathering for parent order: {OrderId}", orderId);
                return BaseResponse<IEnumerable<OrderItemDto>>.Failure("An internal service error occurred while resolving order structure properties.");
            }
        }
    }
}
