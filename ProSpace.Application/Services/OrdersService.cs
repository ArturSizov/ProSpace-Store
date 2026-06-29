using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Mappers;
using ProSpace.Contracts.DTO.Order;
using ProSpace.Contracts.Responses;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Services
{
    public class OrdersService : IOrderService
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<OrdersService> _logger;

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
        /// <param name="validation"></param>
        /// <param name="unitOfWork"></param>
        public OrdersService(ILogger<OrdersService> logger, IUnitOfWork unitOfWork, ISecurityService securityService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _securityService = securityService;
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> CreateAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing business logic authorization parameters for a new order instantiation flow.");

     
                Guid finalCustomerId = customerId;

                if (customerId == Guid.Empty)
                {
                    var authCustomerId = await _securityService.GetCurrentCustomerIdAsync(cancellationToken);
                    if (authCustomerId == null || authCustomerId == Guid.Empty)
                    {
                        _logger.LogWarning("Order creation blocked. Active identity user does not possess an instantiated Customer profile context mapping.");
                        return BaseIdResponse.Failure("Access denied. Customer profile not registered.");
                    }
                    finalCustomerId = authCustomerId.Value;
                }

                var (IsSuccess, Error) = await _securityService.ValidateCustomerAccessAsync(finalCustomerId, cancellationToken);

                if (!IsSuccess)
                    return BaseIdResponse.Failure(Error);

                int maxNumber = await _unitOfWork.OrdersRepository.GetMaxOrderNumberAsync(cancellationToken);

                var domainOrder = new OrderModel
                {
                    Id = Guid.NewGuid(),
                    CustomerId = finalCustomerId,
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    OrderNumber = maxNumber + 1,
                    ShipmentDate = null,
                    Status = "New"
                };

                await _unitOfWork.OrdersRepository.CreateAsync(domainOrder, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Database unit of work flush sequence failed during order initialization for Customer: {Id}", finalCustomerId);
                    return BaseIdResponse.Failure("Failed to save the new order profile due to an underlying infrastructure storage exception.");
                }

                _logger.LogInformation("Order {Id} (Serial Number: {Num}) has been successfully created and saved for Customer {CustId}.",
                    domainOrder.Id, domainOrder.OrderNumber, finalCustomerId);

                return BaseIdResponse.Success(domainOrder.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error while creating order with requested runtime tracking reference ID: {Id}", customerId);
                return BaseIdResponse.Failure("An internal server error occurred while processing the order initialization workflows loops.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating comprehensive order extraction sequence for tracking key target: {Id}", id);

                var existingOrder = await _unitOfWork.OrdersRepository.ReadAsync(id, cancellationToken);

                if (existingOrder == null)
                {
                    _logger.LogWarning("Extraction operation suspended. Order record with ID {Id} was not located.", id);
                    return BaseIdResponse.Failure($"Order with ID {id} not found inside the system registers.");
                }

                var (isSuccess, error) = await _securityService.ValidateCustomerAccessAsync(existingOrder.CustomerId, cancellationToken);

                if (!isSuccess)
                {
                    _logger.LogCritical("Security enforcement blocked an unauthorized extraction trace payload for Order ID: {Id}", id);
                    return BaseIdResponse.Failure(error);
                }

                if (!_securityService.IsManager())
                {
                    if (existingOrder.Status != "New")
                    {
                        _logger.LogWarning("Deletion rejected. Customer attempted to delete Order {Id} with active status: {Status}",
                            id, existingOrder.Status);
                        return BaseIdResponse.Failure("Access denied. Only new unfulfilled orders can be deleted from your personal account.");
                    }
                }

                var cascadingOrderItems = await _unitOfWork.OrderItemsRepository.GetOrderItemsByOrderIdAsync(id, cancellationToken);
                var itemsList = cascadingOrderItems?.ToList() ?? [];

                if (itemsList.Count > 0)
                {
                    _logger.LogInformation("Found {Count} nested order line item rows. Purging cascaded sub-components for Order: {Id}", itemsList.Count, id);

                    foreach (var lineItem in itemsList)
                        await _unitOfWork.OrderItemsRepository.DeleteAsync(lineItem.Id, cancellationToken);
                }

                await _unitOfWork.OrdersRepository.DeleteAsync(id, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Database unit of work flush sequence failed. Cascade execution halted for order extraction sequence: {Id}", id);
                    return BaseIdResponse.Failure("Failed to complete full atomic order profile deletion in persistent storage contexts.");
                }

                _logger.LogInformation("Multi-tier deletion workflow finalized successfully. Order header {Id} and all nested line items wiped entirely.", id);
                return BaseIdResponse.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical unhandled execution exception crashed the multi-tier order deletion pipeline for target ID: {Id}", id);
                return BaseIdResponse.Failure("A critical server exception tracking error occurred during order profile extraction loops.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<IEnumerable<OrderDto>>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating secure global collection scan for orders architecture data layer.");

                if (_securityService.IsManager())
                {
                    _logger.LogInformation("Manager privileges verified successfully. Extracting full system global orders log ledger.");

                    var allOrdersList = await _unitOfWork.OrdersRepository.ReadAllAsync(cancellationToken);
                    var allOrderDtosCollection = allOrdersList?.Select(order => order.ToDto()).ToList() ?? Enumerable.Empty<OrderDto>();

                    return BaseResponse<IEnumerable<OrderDto>>.Success(allOrderDtosCollection);
                }

                var authenticatedCustomerId = await _securityService.GetCurrentCustomerIdAsync(cancellationToken);

                if (authenticatedCustomerId == null || authenticatedCustomerId == Guid.Empty)
                {
                    _logger.LogWarning("Orders compilation terminated. Active user does not possess an initialized Customer profile context mapping.");
                    return BaseResponse<IEnumerable<OrderDto>>.Success(Enumerable.Empty<OrderDto>());
                }

                _logger.LogInformation("Standard customer account verified. Executing isolated database query for Customer ID: {CustomerId}", authenticatedCustomerId);

                var customerSpecificOrdersList = await _unitOfWork.OrdersRepository.GetOrdersByCustomerIdAsync(authenticatedCustomerId.Value, cancellationToken);

                var filteredCustomerOrderDtos = customerSpecificOrdersList?.Select(order => order.ToDto()).ToList() ?? [];

                _logger.LogInformation("Successfully compiled safe isolated orders ledger sequence for Customer profile: {CustomerId}. Total loaded: {Count}",
                    authenticatedCustomerId, filteredCustomerOrderDtos.Count);

                return BaseResponse<IEnumerable<OrderDto>>.Success(filteredCustomerOrderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception structural framework crash disrupted the global orders resolution pipeline execution loops.");
                return BaseResponse<IEnumerable<OrderDto>>.Failure("An internal server error occurred while compiling your orders history records context.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("The process of reading the Order by ID has begun.");

                var order = await _unitOfWork.OrdersRepository.ReadAsync(id, cancellationToken);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {Id} not found inside system registries.", id);
                    return BaseResponse<OrderDto>.Failure($"Order with ID {id} not found.");
                }

                var (isSuccess, error) = await _securityService.ValidateCustomerAccessAsync(order.CustomerId, cancellationToken);

                if (!isSuccess)
                {
                    _logger.LogCritical("Security enforcement blocked unauthorized access attempt to Order ID: {Id}", id);

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
        public async Task<BaseResponse<OrderDto>> UpdateAsync(UpdateOrderDto order, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing administrative updates configuration for order target: {Id}", order.Id);

                var existingOrder = await _unitOfWork.OrdersRepository.ReadAsync(order.Id, cancellationToken);

                if (existingOrder == null)
                {
                    _logger.LogWarning("Modification halted. Order with tracking ID {Id} was not located inside registers.", order.Id);
                    return BaseResponse<OrderDto>.Failure($"Order with ID {order.Id} not found.");
                }

                var (isSuccess, error) = await _securityService.ValidateCustomerAccessAsync(existingOrder.CustomerId, cancellationToken);

                if (!isSuccess)
                {
                    _logger.LogCritical("Security enforcement blocked unauthorized update attempt to Order ID: {Id}", order.Id);
                    return BaseResponse<OrderDto>.Failure($"Order with ID {order.Id} not found.");
                }

                if (!_securityService.IsManager())
                {
                    _logger.LogWarning("Access denied. Customer attempted to bypass UI restrictions to update Order {Id}", order.Id);
                    return BaseResponse<OrderDto>.Failure("Access denied. Only managers possess administrative rights to modify orders.");
                }

                _logger.LogInformation("Manager account update validated for Order {Id}. Applying administrative properties overrides.", order.Id);

                existingOrder.ShipmentDate = order.ShipmentDate;
                existingOrder.Status = order.Status;

                await _unitOfWork.OrdersRepository.UpdateAsync(existingOrder, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                    _logger.LogWarning("No structural data changes were stored for order {Id} (data payload might be identical).", order.Id);
                else
                    _logger.LogInformation("Order {Id} state modification successfully written down to active databases.", existingOrder.Id);

                return BaseResponse<OrderDto>.Success(existingOrder.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled execution lifecycle exception crashed order state synchronization for tracking ID: {Id}", order.Id);
                return BaseResponse<OrderDto>.Failure("An internal service error occurred while processing the order update transaction.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<IEnumerable<OrderDto>>> GetByOrdersCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("The process of reading the Orders by customer code has begun.");

                if (_securityService.IsManager())
                {
                    var allOrdersList = await _unitOfWork.OrdersRepository.GetOrdersByCustomerCodeAsync(customerCode, null, cancellationToken);
                    var allOrderDtosCollection = allOrdersList.Select(order => order.ToDto()).ToList();

                    return BaseResponse<IEnumerable<OrderDto>>.Success(allOrderDtosCollection);
                }

                var authenticatedCustomerId = await _securityService.GetCurrentCustomerIdAsync(cancellationToken);

                if (authenticatedCustomerId == null || authenticatedCustomerId == Guid.Empty)
                {
                    _logger.LogWarning("Orders compilation terminated. Active user context mapping not found.");
                    return BaseResponse<IEnumerable<OrderDto>>.Success([]);
                }

                _logger.LogInformation("Standard customer account verified. Executing isolated database query for Customer Code: {CustomerCode}", customerCode);

                var safeOrdersList = await _unitOfWork.OrdersRepository.GetOrdersByCustomerCodeAsync(customerCode, authenticatedCustomerId.Value, cancellationToken);

                var filteredCustomerOrderDtos = safeOrdersList.Select(order => order.ToDto()).ToList();

                return BaseResponse<IEnumerable<OrderDto>>.Success(filteredCustomerOrderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading orders list for customer code: {CustomerCode}", customerCode);
                return BaseResponse<IEnumerable<OrderDto>>.Failure("Failed to load orders for the specified customer.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<IEnumerable<OrderDto>>> GetOrderByCustomersIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("The process of reading the Orders by customer ID has begun.");

                if (_securityService.IsManager())
                {
                    var allOrdersList = await _unitOfWork.OrdersRepository.GetOrdersByCustomerIdAsync(customerId, cancellationToken);
                    var allOrderDtosCollection = allOrdersList?.Select(order => order.ToDto()).ToList() ?? [];

                    _logger.LogInformation("Manager query: Total orders found by user ID {Id}: {Count}", customerId, allOrderDtosCollection.Count);

                    return BaseResponse<IEnumerable<OrderDto>>.Success(allOrderDtosCollection);
                }

                var authenticatedCustomerId = await _securityService.GetCurrentCustomerIdAsync(cancellationToken);

                if (authenticatedCustomerId == null || authenticatedCustomerId == Guid.Empty)
                {
                    _logger.LogWarning("Orders compilation terminated. Active user context mapping not found.");
                    return BaseResponse<IEnumerable<OrderDto>>.Success([]);
                }

                _logger.LogInformation("Standard customer account verified. Validating requested Customer ID: {CustomerId}", customerId);

                if (authenticatedCustomerId.Value != customerId)
                {
                    _logger.LogWarning("Security Violation! Customer {AuthId} tried to access orders of Customer {RequestedId}",
                        authenticatedCustomerId.Value, customerId);

                    return BaseResponse<IEnumerable<OrderDto>>.Success(Enumerable.Empty<OrderDto>());
                }

                var ordersList = await _unitOfWork.OrdersRepository.GetOrdersByCustomerIdAsync(customerId, cancellationToken);
                var customerOrders = ordersList?.Select(order => order.ToDto()).ToList() ?? [];

                _logger.LogInformation("Total orders found for Customer ID {Id}: {Count}", customerId, customerOrders.Count);

                return BaseResponse<IEnumerable<OrderDto>>.Success(customerOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading orders list for customer ID: {CustomerId}", customerId);
                return BaseResponse<IEnumerable<OrderDto>>.Failure("Failed to load orders for the specified customer.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<OrderDto>> GetByOrderNumberAsync(int orderNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("The process of reading the Order by order number has begun.");

                if (_securityService.IsManager())
                {
                    var fullOrder = await _unitOfWork.OrdersRepository.GetByOrderNumberAsync(orderNumber, cancellationToken);

                    if (fullOrder == null)
                    {
                        _logger.LogWarning("Order with number {Number} not found.", orderNumber);
                        return BaseResponse<OrderDto>.Failure($"Order with number {orderNumber} not found.");
                    }

                    return BaseResponse<OrderDto>.Success(fullOrder.ToDto());
                }

                var authenticatedCustomerId = await _securityService.GetCurrentCustomerIdAsync(cancellationToken);

                if (authenticatedCustomerId == null || authenticatedCustomerId == Guid.Empty)
                {
                    _logger.LogWarning("Order compilation terminated. Active user does not possess an initialized Customer profile context mapping.");
                    return BaseResponse<OrderDto>.Failure("Access denied. Customer profile not registered.");
                }

                _logger.LogInformation("Standard customer account verified. Executing isolated database query for Order Number: {OrderNumber} and Customer ID: {CustomerId}",
                    orderNumber, authenticatedCustomerId.Value);

                var order = await _unitOfWork.OrdersRepository.GetByOrderNumberAsync(orderNumber, cancellationToken);

                if (order == null || order.CustomerId != authenticatedCustomerId.Value)
                {
                    _logger.LogWarning("Order with number: {Number} was not found or access denied for Customer: {CustomerId}.", orderNumber, authenticatedCustomerId.Value);
                    return BaseResponse<OrderDto>.Failure($"Order with number {orderNumber} not found.");
                }

                var orderDto = order.ToDto();

                _logger.LogInformation("Order loaded successfully: {Number}", orderDto.OrderNumber);

                return BaseResponse<OrderDto>.Success(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for order by order number: {Number}", orderNumber);
                return BaseResponse<OrderDto>.Failure("Unable to search for order due to an internal error.");
            }
        }
    }
}
