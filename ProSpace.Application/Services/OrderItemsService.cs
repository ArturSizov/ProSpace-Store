using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Mappers;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;
using ProSpace.Domain.Models;
using System.Security.Claims;

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
        private readonly IValidator<OrderItemDto> _validation;

        /// <summary>
        /// Unit of Work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Http context accessor
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="validation"></param>
        /// <param name="unitOfWork"></param>
        public OrderItemsService(ILogger<OrderItemsService> logger, IValidator<OrderItemDto> validation, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _validation = validation;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> CreateAsync(OrderItemDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing order item creation sequence for product {ItemId} inside Order {OrderId}", dto.ItemId, dto.OrderId);

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

                var customer = await _unitOfWork.CustomersRepository.ReadAsync(parentOrder.CustomerId, cancellationToken);
                if (customer == null)
                {
                    _logger.LogWarning("Creation rejected. Customer profile linked to order ID {OrderId} (CustomerKey: {CustomerId}) not found.",
                        dto.OrderId, parentOrder.CustomerId);
                    return BaseIdResponse.Failure("The customer profile associated with this order container was not found.");
                }

                var accessCheck = await ValidateOrderOwnershipAsync(parentOrder.CustomerId);

                if (!accessCheck.IsSuccess)
                {
                    _logger.LogCritical("Security breach attempt blocked! Unauthorized user tried to insert item line into Order: {OrderId}", dto.OrderId);
                    return BaseIdResponse.Failure(accessCheck.Error);
                }

                decimal singleUnitDiscountedPrice = catalogItem.Price * (1m - (customer.Discount / 100m));

                var domainOrderItem = new OrderItemModel
                {
                    Id = dto.Id,
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

                var item = await _unitOfWork.OrderItemsRepository.ReadAsync(id, cancellationToken);

                if (item == null)
                {
                    _logger.LogWarning("Extraction operation suspended. Order item with ID {Id} was not located.", id);
                    return BaseIdResponse.Failure($"Order item with ID {id} not found inside system registers.");
                }

                var parentOrder = await _unitOfWork.OrdersRepository.ReadAsync(item.OrderId, cancellationToken);

                if (parentOrder == null)
                {
                    _logger.LogError("Database integrity violation: Order item {Id} points to non-existent Order {OrderId}.",
                        item.Id, item.OrderId);
                    return BaseIdResponse.Failure("Internal reference consistency breakdown. Parent order container missing.");
                }

                var (isSuccess, error) = await ValidateOrderOwnershipAsync(parentOrder.CustomerId);

                if (!isSuccess)
                {
                    _logger.LogCritical("Security breach attempt blocked! Unauthorized user tried to delete Order Item {ItemId} from Order {OrderId}",
                        id, parentOrder.Id);
                    return BaseIdResponse.Failure(error);
                }

                var currentUser = _httpContextAccessor.HttpContext?.User;

                if (currentUser != null && !currentUser.IsInRole("manager") && !currentUser.IsInRole("Manager"))
                {
                    if (parentOrder.Status != "New")
                    {
                        _logger.LogWarning("Extraction operation rejected. Customer tried to delete Item {ItemId} from active Order {OrderId} with status: {Status}",
                            id, parentOrder.Id, parentOrder.Status);
                        return BaseIdResponse.Failure("Access denied. Items can only be extracted from new unfulfilled orders.");
                    }
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

                var currentUser = _httpContextAccessor.HttpContext?.User;

                if (currentUser != null && (currentUser.IsInRole("manager") || currentUser.IsInRole("Manager")))
                {
                    _logger.LogInformation("Manager privileges verified. Fetching the complete database sequence.");

                    var allOrderItems = await _unitOfWork.OrderItemsRepository.ReadAllAsync(cancellationToken);
                    var allDtos = allOrderItems?.Select(item => item.ToDto()) ?? [];

                    return BaseResponse<IEnumerable<OrderItemDto>>.Success(allDtos);
                }

                var nameIdentifierClaim = currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(nameIdentifierClaim, out var authenticatedUserId))
                {
                    _logger.LogError("Security system breakdown: Unable to resolve a valid Guid from user NameIdentifier claim.");
                    return BaseResponse<IEnumerable<OrderItemDto>>.Failure("Access denied. Invalid user identity configuration tokens.");
                }

                var customerProfile = await _unitOfWork.CustomersRepository.GetByAppUserIdAsync(authenticatedUserId, cancellationToken);

                if (customerProfile == null)
                {
                    _logger.LogWarning("Order items gathering halted. Customer profile not found for identity user: {UserId}", authenticatedUserId);
                    return BaseResponse<IEnumerable<OrderItemDto>>.Success([]);
                }

                var accessCheck = await ValidateOrderOwnershipAsync(customerProfile.Id);
                if (!accessCheck.IsSuccess)
                    return BaseResponse<IEnumerable<OrderItemDto>>.Failure(accessCheck.Error);

                var customerSpecificOrderItems = await _unitOfWork.OrderItemsRepository.GetOrderItemsByCustomerIdAsync(customerProfile.Id, cancellationToken);
                var customerDtos = customerSpecificOrderItems?.Select(item => item.ToDto()) ?? [];

                return BaseResponse<IEnumerable<OrderItemDto>>.Success(customerDtos);
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
                _logger.LogInformation("Processing architectural updates configuration for order item target: {Id}", orderItem.Id);

                var existingOrderItem = await _unitOfWork.OrderItemsRepository.ReadAsync(orderItem.Id, cancellationToken);
                if (existingOrderItem == null)
                {
                    _logger.LogWarning("Modification halted. Order item with tracking ID {Id} was not located inside registers.", orderItem.Id);
                    return BaseResponse<OrderItemDto>.Failure($"Order item with ID {orderItem.Id} not found.");
                }

                var parentOrder = await _unitOfWork.OrdersRepository.ReadAsync(existingOrderItem.OrderId, cancellationToken);
                if (parentOrder == null)
                {
                    _logger.LogError("Database integrity failure: Order item {Id} points to a missing parent Order {OrderId}", existingOrderItem.Id, existingOrderItem.OrderId);
                    return BaseResponse<OrderItemDto>.Failure("Internal database reference consistency error.");
                }

                var accessCheck = await ValidateOrderOwnershipAsync(parentOrder.CustomerId);
                if (!accessCheck.IsSuccess)
                {
                    _logger.LogCritical("Security breach attempt! Unauthorized item modification blocked for Order Item Node: {Id}", orderItem.Id);
                    return BaseResponse<OrderItemDto>.Failure(accessCheck.Error);
                }

                var currentUser = _httpContextAccessor.HttpContext?.User;
                decimal determinedFinalPrice = existingOrderItem.ItemPrice;

                if (currentUser != null && !currentUser.IsInRole("manager") && !currentUser.IsInRole("Manager"))
                {
                    _logger.LogInformation("Standard customer account update detected for item {Id}. Retaining existing historical price: {Price}",
                        orderItem.Id, existingOrderItem.ItemPrice);

                    var customer = await _unitOfWork.CustomersRepository.ReadAsync(parentOrder.CustomerId, cancellationToken);
                    var catalogItem = await _unitOfWork.ItemsRepository.ReadAsync(existingOrderItem.ItemId, cancellationToken);

                    if (customer != null && catalogItem != null)
                        determinedFinalPrice = catalogItem.Price * (1m - (customer.Discount / 100m));

                }
                else
                {
                    _logger.LogInformation("Manager account update detected for item {Id}. Applying requested custom price parameter: {Price}",
                        orderItem.Id, orderItem.ItemPrice);

                    determinedFinalPrice = orderItem.ItemPrice;
                }

                existingOrderItem.ItemsCount = orderItem.ItemsCount;
                existingOrderItem.ItemId = orderItem.ItemId;
                existingOrderItem.ItemPrice = determinedFinalPrice; 

                await _unitOfWork.OrderItemsRepository.UpdateAsync(existingOrderItem, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                    _logger.LogWarning("No structural data changes were stored for order item {Id} (data payload might be identical).", orderItem.Id);

                else
                    _logger.LogInformation("Order item {Id} state modification successfully written down to active databases.", existingOrderItem.Id);

                return BaseResponse<OrderItemDto>.Success(existingOrderItem.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled execution lifecycle exception crashed order item state synchronization for tracking ID: {Id}", orderItem.Id);
                return BaseResponse<OrderItemDto>.Failure("An internal service error occurred while processing the order item update transaction.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<IEnumerable<OrderItemDto>>> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing secure data isolation routing layout scans for order items under Order: {OrderId}", orderId);

                var currentUser = _httpContextAccessor.HttpContext?.User;

                var parentOrder = await _unitOfWork.OrdersRepository.ReadAsync(orderId, cancellationToken);

                if (parentOrder == null)
                {
                    _logger.LogWarning("Order items lookup suspended. Parent Order key tracking node {OrderId} not found.", orderId);
                    return BaseResponse<IEnumerable<OrderItemDto>>.Failure($"Order with tracking identifier {orderId} does not exist.");
                }

                var customer = await _unitOfWork.CustomersRepository.ReadAsync(parentOrder.CustomerId, cancellationToken);

                if (customer == null)
                {
                    _logger.LogWarning("Order item search suspended. Customer profile not found. {CustomerId}", parentOrder.CustomerId);
                    return BaseResponse<IEnumerable<OrderItemDto>>.Failure($"Customer profile with ID {parentOrder.CustomerId} does not exist.");
                }

                if (currentUser != null && !currentUser.IsInRole("manager") && !currentUser.IsInRole("Manager"))
                {
                    var nameIdentifierClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!Guid.TryParse(nameIdentifierClaim, out var authenticatedUserId))
                    {
                        _logger.LogError("Security system breakdown: Unable to resolve a valid Guid from user NameIdentifier claim context.");
                        return BaseResponse<IEnumerable<OrderItemDto>>.Failure("Access denied. Invalid user identity configuration.");
                    }

                    if (authenticatedUserId != customer.AppUserId)
                    {
                        _logger.LogCritical("Security breach attempt blocked! Authenticated standard user {AuthId} tried to view line items for Order {OrderId} owned by user {OwnerId}.",
                            authenticatedUserId, orderId, customer.AppUserId);

                        return BaseResponse<IEnumerable<OrderItemDto>>.Failure("Access denied. You are not authorized to view components of this order.");
                    }
                }

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

        /// <summary>
        /// Validates whether the currently authenticated user has rights to modify or view data tied to a specific Customer ID.
        /// Managers are always allowed; standard customers are restricted strictly to their own accounts.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer owning the target order resource.</param>
        /// <returns>A tuple indicating success status and an optional localized error message text string description.</returns>
        private async Task<(bool IsSuccess, string Error)> ValidateOrderOwnershipAsync(Guid customerId)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser != null && (currentUser.IsInRole("manager") || currentUser.IsInRole("Manager")))
                return (true, string.Empty);


            var nameIdentifierClaim = currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(nameIdentifierClaim, out var authenticatedUserId))
            {
                _logger.LogError("Security system breakdown: Unable to resolve a valid Guid from user NameIdentifier claim context.");
                return (false, "Access denied. Invalid user identity configuration parameters.");
            }

            var customerProfile = await _unitOfWork.CustomersRepository.ReadAsync(customerId);

            if (customerProfile == null)
            {
                _logger.LogWarning("Access evaluation suspended. Parent Customer record {Id} was not located in database registers.", customerId);
                return (false, "Associated customer account data profile was not found.");
            }

            if (authenticatedUserId != customerProfile.AppUserId)
            {
                _logger.LogCritical("Security breach attempt blocked! Authenticated standard user {AuthId} tried to access data owned by Customer profile {CustomerId} (Owned by user: {OwnerId}).",
                    authenticatedUserId, customerId, customerProfile.AppUserId);

                return (false, "Access denied. You do not possess structural permissions to view or modify this resource.");
            }

            return (true, string.Empty);
        }
    }
}
