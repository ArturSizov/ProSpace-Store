using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Contracts.Contracts.Request.Order;
using ProSpace.Contracts.DTO.Order;
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
        /// Creates a new order for an authorized customer.
        /// </summary>
        /// <param name="ct">Cancellation token to abort the operation.</param>
        /// <returns>A standard response containing the created order ID on success.</returns>
        [HttpPost]
        [Authorize(Roles = "customer, Customer")]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var targetOrder = new OrderDto
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.Empty
            };

            return ProcessResponse(await _service.CreateAsync(Guid.Empty, ct));
        }

        /// <summary>
        /// Creates an order by a manager on behalf of a specific customer.
        /// </summary>
        /// <param name="request">The request body payload containing the customer identifier.</param>
        /// <param name="ct">Cancellation token to abort the operation.</param>
        /// <returns>A standard response containing the created order ID on success.</returns>
        [HttpPost("admin/orders")]
        [Authorize(Roles = "manager, Manager")]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateByManager([FromBody] MamagerCreateOrderRequest request, CancellationToken ct)
        {
            var targetOrder = new OrderDto
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId
            };

            return ProcessResponse(await _service.CreateAsync(request.CustomerId, ct));
        }

        /// <summary>
        /// Retrieves the profile details of a specific order header using its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the matching order payload inside the Data field.</returns>
        /// <response code="200">The order record was successfully located and fetched.</response>
        /// <response code="404">No order was found matching the provided identifier.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => ProcessResponse(await _service.ReadAsync(id, ct));

        /// <summary>
        /// Retrieves a complete list of all orders registered in the system database.
        /// </summary>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the array of orders inside the Data field.</returns>
        /// <response code="200">The list of orders was successfully fetched (may return an empty array if no records exist).</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto[]>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => ProcessResponse(await _service.ReadAllAsync(ct));

        /// <summary>
        /// Updates the core details (dates, numbers, status) of an existing order header.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order being updated. Must match the ID inside the request body.</param>
        /// <param name="request">The order data transfer object containing the updated fields.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the updated order details inside the Data field.</returns>
        /// <response code="200">The order was successfully located, validated, and updated in the system.</response>
        /// <response code="400">The route ID does not match the body ID, or the input data failed validation rules.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        /// <response code="404">The order with the specified identifier was not found in the database.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "manager, Manager")]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderRequest request, CancellationToken ct)
        {
            var targetOrder = new UpdateOrderDto
            {
                Id = id,
                Status = request.Status,
                ShipmentDate = request.ShipmentDate,
            };

            return ProcessResponse(await _service.UpdateAsync(targetOrder, ct));
        }

        /// <summary>
        /// Permanently deletes an order header and its operational history from the database.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the order to remove.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper verifying completion via the Id field.</returns>
        /// <response code="200">The order was found and deleted from the persistence layer.</response>
        /// <response code="404">No order matched the provided identity to perform deletion.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
            => ProcessResponse(await _service.DeleteAsync(id, ct));

        /// <summary>
        /// Searches for and retrieves a single order profile using its unique human-readable number.
        /// </summary>
        /// <param name="number">The sequential integer number assigned to the target order.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the matching order details.</returns>
        /// <response code="200">The order with the requested number was found.</response>
        /// <response code="404">No order exists with the specified number.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpGet("by-number/{number:int}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByNumber(int number, CancellationToken ct)
            => ProcessResponse(await _service.GetByOrderNumberAsync(number, ct));


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
        [ProducesResponseType(typeof(BaseResponse<OrderDto[]>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByCustomerCode(string customerCode, CancellationToken ct)
            => ProcessResponse(await _service.GetByOrdersCustomerCodeAsync(customerCode, ct));

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
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByCustomerId(Guid customerId, CancellationToken ct)
            => ProcessResponse(await _service.GetByCustomersIdAsync(customerId, ct));
    }
}
