using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Api.Controllers
{
    /// <summary>
    /// Provides HTTP endpoints for managing order headers and processing customer sales orders.
    /// </summary>
    public class OrdersController : BaseApiController
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<OrdersController> _logger;

        /// <summary>
        /// Orders service
        /// </summary>
        private readonly IOrderService _service;


        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class.
        /// </summary>
        /// <param name="logger">The application logger infrastructure.</param>
        /// <param name="service">The service handling core order business logic.</param>
        public OrdersController(ILogger<OrdersController> logger, IOrderService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// Creates and registers a new order in the system.
        /// </summary>
        /// <param name="dto">The order data transfer object containing customer and date specifications.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the generated ID of the new order.</returns>
        /// <response code="200">The order was successfully validated and scheduled for creation.</response>
        /// <response code="400">Validation failed or input data was corrupted.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<CreateOrderDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
        {
            var response = await _service.CreateAsync(dto, ct);
            return ProcessResponse(response);
        }

        /// <summary>
        /// Retrieves the profile details of a specific order header using its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the matching order payload inside the Data field.</returns>
        /// <response code="200">The order record was successfully located and fetched.</response>
        /// <response code="404">No order was found matching the provided identifier.</response>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var response = await _service.ReadAsync(id, ct);
            return ProcessResponse(response);
        }

        /// <summary>
        /// Retrieves a complete list of all orders registered in the system database.
        /// </summary>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the array of orders inside the Data field.</returns>
        /// <response code="200">The list of orders was successfully fetched (may return an empty array if no records exist).</response>
        [HttpGet]
        [Authorize(Roles = "manager,Manager")]
        [ProducesResponseType(typeof(BaseResponse<OrderDto[]>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var response = await _service.ReadAllAsync(ct);
            return ProcessResponse(response);
        }

        /// <summary>
        /// Updates the core details (dates, numbers, status) of an existing order header.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order being updated. Must match the ID inside the request body.</param>
        /// <param name="dto">The order data transfer object containing the updated fields.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the updated order details inside the Data field.</returns>
        /// <response code="200">The order was successfully located, validated, and updated in the system.</response>
        /// <response code="400">The route ID does not match the body ID, or the input data failed validation rules.</response>
        /// <response code="404">The order with the specified identifier was not found in the database.</response>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderDto dto, CancellationToken ct)
        {
            if (id != dto.Id)
            {
                _logger.LogWarning("Route ID {RouteId} does not match Request Body ID {BodyId}", id, dto.Id);
                return BadRequest(BaseResponse<OrderDto>.Failure("The URL identifier does not match the provided object data identifier."));
            }

            var response = await _service.UpdateAsync(dto, ct);
            return ProcessResponse(response);
        }

        /// <summary>
        /// Permanently deletes an order header and its operational history from the database.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order to remove.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper verifying completion via the Id field.</returns>
        /// <response code="200">The order was found and deleted from the persistence layer.</response>
        /// <response code="404">No order matched the provided identity to perform deletion.</response>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var response = await _service.DeleteAsync(id, ct);
            return ProcessResponse(response);
        }

        /// <summary>
        /// Searches for and retrieves a single order profile using its unique human-readable number.
        /// </summary>
        /// <param name="number">The sequential integer number assigned to the target order.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the matching order details.</returns>
        /// <response code="200">The order with the requested number was found.</response>
        /// <response code="404">No order exists with the specified number.</response>
        [HttpGet("by-number/{number:int}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNumber(int number, CancellationToken ct)
        {
            var response = await _service.GetByOrderNumberAsync(number, ct);
            return ProcessResponse(response);
        }


        /// <summary>
        /// Retrieves all orders associated with a specific customer using their unique business code.
        /// </summary>
        /// <param name="customerCode">The unique business code string of the customer.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the array of matching orders inside the Data field.</returns>
        /// <response code="200">The collection of orders for the specified customer was successfully fetched (can be empty).</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpGet("by-customer-code/{customerCode}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto[]>), StatusCodes.Status200OK)] // ИСПРАВЛЕНО: Изменен дженерик на массив OrderDto[]
        [ProducesResponseType(typeof(BaseResponse<OrderDto[]>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByCustomerCode(string customerCode, CancellationToken ct)
        {
            var response = await _service.GetByCustomerCodeAsync(customerCode, ct);
            return ProcessResponse(response);
        }

        /// <summary>
        /// Retrieves all orders associated with a specific customer using their unique identifier.
        /// </summary>
        /// <param name="customerId">The unique identifier (GUID) of the customer.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the array of matching orders inside the Data field.</returns>
        /// <response code="200">The collection of orders for the specified customer ID was successfully fetched (can be empty).</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpGet("by-customer-id/{customerId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto[]>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<OrderDto[]>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByCustomerId(Guid customerId, CancellationToken ct)
        {
            var response = await _service.GetByCustomerIdAsync(customerId, ct);
            return ProcessResponse(response);
        }
    }
}
