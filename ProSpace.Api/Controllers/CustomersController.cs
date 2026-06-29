using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProSpace.Api.Contracts.Request;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Contracts.DTO.Customer;
using ProSpace.Contracts.Responses;

namespace ProSpace.Api.Controllers
{
    /// <summary>
    /// Provides HTTP endpoints for managing customer profiles and accounts, including registration, updates, and profile lookup.
    /// </summary>
    public class CustomersController : BaseApiController
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<CustomersController> _logger;

        /// <summary>
        /// Custpmer service
        /// </summary>
        private readonly ICustomersService _service;

        /// <summary>
        ///  Initializes a new instance of the <see cref="CustomersController"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        public CustomersController(ILogger<CustomersController> logger, ICustomersService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// Registers a new customer and creates their security credentials.
        /// </summary>
        /// <param name="request">The customer registration request containing identity and initial profile details.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper indicating registration success or containing validation logs.</returns>
        /// <response code="200">The account and customer profile were successfully created.</response>
        /// <response code="400">The input fields failed application validation or identity registration constraints.</response>
        [HttpPost]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] CustomerRegisterRequest request, CancellationToken ct)
        {
            var targetDto = new RegisterCustomerDto
            {
                Email = request.Email,
                Password = request.Password,
                Address = request.Address,
                Discount = 0,
                UserName = request.UserName
            };

            return ProcessResponse(await _service.RegisterCustomerAsync(targetDto, role: "customer", cancellationToken: ct));
        }

        /// <summary>
        /// Retrieves the profile details of a specific customer using their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the customer record.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the matching customer payload inside the Data field.</returns>
        /// <response code="200">The customer record was located and fetched successfully.</response>
        /// <response code="404">No customer was found matching the provided identifier.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<CustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct) 
            => ProcessResponse(await _service.ReadAsync(id, ct));

        /// <summary>
        /// Retrieves a complete list of all registered customers in the system.
        /// </summary>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the array of customers inside the Data field.</returns>
        /// <response code="200">The list of customers was fetched successfully.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpGet("admin")]
        [Authorize(Roles = "manager,Manager")]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<CustomerDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(CancellationToken ct) 
            => ProcessResponse(await _service.ReadAllAsync(ct));

        /// <summary>
        /// Updates a customer's personal profile information. Restricted to the profile owner.
        /// </summary>
        /// <remarks>
        /// This endpoint extracts the customer identifier directly from the secure URL route to guarantee REST conformity. 
        /// Inbound JSON payloads are strictly limited to contact details (Name and Address). 
        /// Administrative and commercial fields (such as Code and Discount) are excluded from the request structure 
        /// to provide compile-time defense against over-posting parameter tampering.
        /// </remarks>
        /// <param name="request">The customer-safe payload defining requested personal information mutations.</param>
        /// <param name="ct">The asynchronous operations lifecycle cancellation token reference.</param>
        /// <returns>An IActionResult containing the successfully updated customer profile data or an error response block.</returns>
        [HttpPut("me")]
        [Authorize(Roles = "customer, Customer")]
        [ProducesResponseType(typeof(BaseResponse<CustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateByCustomer([FromBody] UpdateCustomerRequest request, CancellationToken ct)
        {
            var targetDto = new CustomerDto
            {
                Name = request.Name,
                Address = request.Address
            };

            return ProcessResponse(await _service.UpdateAsync(targetDto, ct));
        }

        /// <summary>
        /// Administratively updates a customer profile configuration, enabling code and discount modifications.
        /// </summary>
        /// <remarks>
        /// This endpoint maps the resource identifier from the secure URL route with the administrative payload from the request body. 
        /// Restricted exclusively to Managerial roles, it allows direct mutation of all customer fields, 
        /// including critical business parameters such as client corporate codes and customized loyalty discounts.
        /// </remarks>
        /// <param name="id">The unique Guid tracking key of the targeted customer profile located in the URL path parameters.</param>
        /// <param name="request">The manager payload defining structural contact adjustments along with commercial override parameters.</param>
        /// <param name="ct">The asynchronous operations lifecycle cancellation token reference.</param>
        /// <returns>An IActionResult containing the administratively synchronized customer profile data or an error response block.</returns>
        [HttpPut("admin/{id:guid}")]
        [Authorize(Roles = "manager, Manager")]
        [ProducesResponseType(typeof(BaseResponse<CustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateByManager([FromRoute] Guid id, [FromBody] UpdateManagerRequest request, CancellationToken ct)
        {
            var targetDto = new CustomerDto
            {
                Id = id,
                Code = request.Code,
                Name = request.Name,
                Address = request.Address,
                Discount = request.Discount
            };

            return ProcessResponse(await _service.UpdateAsync(targetDto, ct));
        }


        /// <summary>
        /// Permanently deletes a customer profile and removes their associated authorization system identity account.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the customer to erase.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper verifying completion via the Id field.</returns>
        /// <response code="200">The customer record and its authentication context were successfully deleted.</response>
        /// <response code="404">No customer matched the provided identity to execute the deletion chain.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpDelete("admin/{id:guid}")]
        [Authorize(Roles = "manager,Manager")]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct) 
            => ProcessResponse(await _service.DeleteAsync(id, ct));

        /// <summary>
        /// Searches for and retrieves a customer profile using their unique email address link.
        /// </summary>
        /// <param name="email">The unique email address string tied to the target account.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper with the customer data inside the Data field.</returns>
        /// <response code="200">The customer profile was located via email reference parameters.</response>
        /// <response code="404">No customer profile exists under the requested email credentials.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpGet("by-email/{email}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<CustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByEmailAsync(string email, CancellationToken ct)
            => ProcessResponse(await _service.GetByEmailAsync(email, ct));

    }
}
