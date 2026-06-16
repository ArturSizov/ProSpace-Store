using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Api.Controllers
{
    /// <summary>
    /// API controller managing endpoints for individual order line items operations.
    /// </summary>
    public class OrderItemsController : BaseApiController
    {
        /// <summary>
        /// The logger instance for diagnostic messages and analytics.
        /// </summary>
        private readonly ILogger<OrderItemsController> _logger;

        /// <summary>
        /// The application service handling business workflows for order items.
        /// </summary>
        private readonly IOrderItemsService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderItemsController"/> class.
        /// </summary>
        /// <param name="logger">The system diagnostic logger implementation dependency injection wrapper.</param>
        /// <param name="service">The business logic implementation contract for managing order entry rows.</param>
        public OrderItemsController(ILogger<OrderItemsController> logger, IOrderItemsService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// Creates a new order item entry inside the system database storage registry.
        /// </summary>
        /// <param name="dto">The data transfer object data payload containing configuration specs for the entry.</param>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>A resource payload context layout showing the newly instantiated target entity tracking ID.</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)] // Changed to BaseIdResponse
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)] // Changed to base BaseResponse
        public async Task<IActionResult> Create([FromBody] CreateOrderItemDto dto, CancellationToken ct) 
            => ProcessResponse(await _service.CreateAsync(dto, ct));

        /// <summary>
        /// Retrieves a specific order item record utilizing its core identifier key lookup property.
        /// </summary>
        /// <param name="id">The targeting entity global unique database identifier asset tracker.</param>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>A resource data model wrapping structural data payload properties if match targets exist.</returns>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)] // Changed to base BaseResponse
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct) 
            => ProcessResponse(await _service.ReadAsync(id, ct));

        /// <summary>
        /// Retrieves a complete flat collection sequence of all active configured system order line objects.
        /// </summary>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>A list collection matrix delivering full active contextual entries.</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<OrderItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => ProcessResponse(await _service.ReadAllAsync(ct));

        /// <summary>
        /// Reconfigures and overrides mutable parameters of a target persistent database entry.
        /// </summary>
        /// <param name="id">The unique validation key parameter provided inside the path URL routing template.</param>
        /// <param name="dto">The updated metadata container carrying incoming operational payload changes.</param>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>An action outcome showing completed state synchronization variables.</returns>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderItemDto dto, CancellationToken ct)
        {
            if (id != dto.Id)
            {
                _logger.LogWarning("Route ID {RouteId} does not match Request Body ID {BodyId}", id, dto.Id);
                return BadRequest(BaseResponse.Failure("The URL identifier does not match the provided object data identifier."));
            }

            var response = await _service.UpdateAsync(dto, ct);
            return ProcessResponse(response);
        }

        /// <summary>
        /// Purges a definitive tracking item context layer permanently out of active record files.
        /// </summary>
        /// <param name="id">The targeted lookup structural entity identifier key assigned for extraction.</param>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>An tracking notification packet signaling extraction transaction status metrics.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)] // Changed to BaseIdResponse
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)] // Changed to base BaseResponse
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var response = await _service.DeleteAsync(id, ct);
            return ProcessResponse(response);
        }

        /// <summary>
        /// Fetches all active sub-component items assigned under a parent order entity aggregation root.
        /// </summary>
        /// <param name="orderId">The parent transaction group lookup tracker node context indicator value.</param>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>An extracted array listing items configured underneath the requested order group node.</returns>
        [HttpGet("by-order-id/{orderId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<OrderItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByCustomerId(Guid orderId, CancellationToken ct)
        {
            var response = await _service.GetOrderItemsByOrderIdAsync(orderId, ct);
            return ProcessResponse(response);
        }
    }
}
