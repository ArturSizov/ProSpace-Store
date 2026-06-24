using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProSpace.Api.Contracts.Request;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Api.Controllers
{
    /// <summary>
    /// API controller managing endpoints for individual order line items operations.
    /// Provides secure endpoints to handle creation, retrieval, modification, and deletion of order sub-components.
    /// </summary>
    public class OrderItemsController : BaseApiController
    {
        /// <summary>
        /// The logger instance for diagnostic messages, validation warnings, and processing metrics traces.
        /// </summary>
        private readonly ILogger<OrderItemsController> _logger;

        /// <summary>
        /// The application service handling business workflows and validation execution loops for order items.
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
        /// <param name="request">The data transfer object data payload containing configuration specs for the entry.</param>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>A resource payload context layout showing the newly instantiated target entity tracking ID.</returns>
        /// <response code="200">The target order item fields were located, mapped, and created securely.</response>
        /// <response code="400">The inbound payload parameters failed business rule validations or format schema checks.</response>
        /// <response code="401">The request lacks valid authentication credentials tokens context.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] OrderItemRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Mapping incoming create item request for Order ID: {OrderId}", request.OrderId);

            var targetDto = new OrderItemDto
            {
                Id = Guid.NewGuid(),
                ItemId = request.ItemId,
                ItemsCount = request.ItemsCount,
                OrderId = request.OrderId,
                ItemPrice = 0
            };

            return ProcessResponse(await _service.CreateAsync(targetDto, ct));
        }

        /// <summary>
        /// Retrieves a specific order item record utilizing its core identifier key lookup property.
        /// </summary>
        /// <param name="id">The targeting entity global unique database identifier asset tracker.</param>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>A resource data model wrapping structural data payload properties if match targets exist.</returns>
        /// <response code="200">The order item entry was located and retrieved successfully.</response>
        /// <response code="404">No order item record matched the provided unique identifier.</response>
        /// <response code="401">The request lacks valid authentication credentials tokens context.</response>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => ProcessResponse(await _service.ReadAsync(id, ct));

        /// <summary>
        /// Retrieves a complete flat collection sequence of all active configured system order line objects.
        /// </summary>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>A list collection matrix delivering full active contextual entries.</returns>
        /// <response code="200">The collection sequence of order items was compiled and returned successfully.</response>
        /// <response code="401">The request lacks valid authentication credentials tokens context.</response>
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
        /// <param name="request">The updated metadata container carrying incoming operational payload changes.</param>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>An action outcome showing completed state synchronization variables.</returns>
        /// <response code="200">The target order item configurations were updated and synchronized securely.</response>
        /// <response code="400">The input property layout failed data type structures or business criteria limitations.</response>
        /// <response code="404">The requested order item identifier key node was not located in database cache registers.</response>
        /// <response code="401">The request lacks valid authentication credentials tokens context.</response>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderItemRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Compiling incoming modification payload parameters for Order Item Node: {Id}", id);

            var targetItem = new OrderItemDto
            {
                Id = id,
                OrderId = request.OrderId,
                ItemsCount = request.ItemsCount,
                ItemId = request.ItemId,
                ItemPrice = 0
            };

            return ProcessResponse(await _service.UpdateAsync(targetItem, ct));
        }

        /// <summary>
        /// Purges a definitive tracking item context layer permanently out of active record files.
        /// </summary>
        /// <param name="id">The targeted lookup structural entity identifier key assigned for extraction.</param>
        /// <param name="ct">The operational lifecycle token monitoring for cancellation signals.</param>
        /// <returns>An tracking notification packet signaling extraction transaction status metrics.</returns>
        /// <response code="200">The order item records were extracted and wiped out completely.</response>
        /// <response code="404">No matching order item trace node was found to execute destruction commands.</response>
        /// <response code="401">The request lacks valid authentication credentials tokens context.</response>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
            => ProcessResponse(await _service.DeleteAsync(id, ct));
    }
}