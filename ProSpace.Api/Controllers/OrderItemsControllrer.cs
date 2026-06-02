using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProSpace.Api.Contracts.Request;
using ProSpace.Api.Contracts.Response;
using ProSpace.Domain.Interfaces.Services;
using ProSpace.Domain.Models;
using ProSpace.Application.Services;

namespace ProSpace.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderItemsControllrer : ControllerBase
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<OrderItemsControllrer> _logger;

        /// <summary>
        /// Order items service
        /// </summary>
        private readonly IOtderItemsService _service;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        public OrderItemsControllrer(ILogger<OrderItemsControllrer> logger, IOtderItemsService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// Get order items response
        /// </summary>
        /// <returns></returns>
        [HttpGet("/order-items")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<List<OrderItemResponse>>> GetAllOrderItemsAsync()
        {
            try
            {
                var response = await _service.ReadAllAsync();

                _logger.LogInformation($"Count: {response?.Count()}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/order-items/{orderId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetOrderItemsByOrderIdAsync(Guid orderId)
        {
            try
            {
                var orderItems = await _service.GetOrderItemsByOrderIdAsync(orderId);

                _logger.LogInformation(orderItems?.Length.ToString());
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Create order item
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("/create/order-item")]
        [Authorize]
        public async Task<IActionResult> CreateOrderItemAsync([FromBody] OrderItemRequest request)
        {
            try
            {
                var orderItem = new OrderItemModel
                {
                    OrderId = request.OrderId,
                    ItemId = request.ItemId, 
                    ItemsCount = request.ItemsCount, 
                    ItemPrice = request.ItemPrice 
                };

                var result = await _service.CreateAsync(orderItem);

                if (result.Item1 == null)
                {
                    _logger.LogError("Failed to create order item");

                    return BadRequest(result.Item2);
                }

                _logger.LogInformation($"Order item created");

                return Ok(result.Item1);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update item reques
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("/update/order-item/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateOrderItemAsync(Guid id, [FromBody] OrderItemRequest request)
        {
            try
            {
                var orderItem = new OrderItemModel
                {
                    Id = id,
                    OrderId = request.OrderId, 
                    ItemId = request.ItemId, 
                    ItemsCount = request.ItemsCount, 
                    ItemPrice = request.ItemPrice
                };

                var newOrderItem = await _service.UpdateAsync(orderItem);

                if(newOrderItem == null)
                {
                    _logger.LogError("Error updated order item");
                    return BadRequest("Error updated order item");
                }

                _logger.LogInformation($"Order item updated");

                return Ok(newOrderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete order item 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("/delete/order-item/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteOrderItemAsync(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result)
                {
                    _logger.LogError("Failed to remove order item");
                    return BadRequest("Failed to remove order item");
                }

                _logger.LogInformation("Order item removed");

                return Ok($"Order item {id} removed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

    }
}
