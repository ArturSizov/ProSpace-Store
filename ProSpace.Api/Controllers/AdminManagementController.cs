using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProSpace.Api.Contracts.Request;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Api.Controllers
{
    /// <summary>
    /// Controller for administrative staff management tasks.
    /// </summary>
    [ApiController]
    [Route("api/admin/management")]
    [Produces("application/json")]
    [Authorize(Roles = "manager,Manager")]
    public class AdminManagementController : ControllerBase
    {
        /// <summary>
        /// The logger instance for recording diagnostic messages and application events.
        /// </summary>
        private readonly ILogger<AdminManagementController> _logger;

        /// <summary>
        /// The core service layer handling domain logic for system customer actions.
        /// </summary>
        private readonly ICustomersService _customersService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminManagementController"/> class.
        /// </summary>
        /// <param name="logger">The infrastructure logging service dependency injection wrapper.</param>
        /// <param name="customersService">The domain application logic contract reference for customer profiles.</param>
        public AdminManagementController(ILogger<AdminManagementController> logger, ICustomersService customersService)
        {
            _logger = logger;
            _customersService = customersService;
        }

        /// <summary>
        /// Registers a new elevated internal workflow employee with specific functional permission scopes.
        /// </summary>
        /// <param name="request">The structure containing identity login data credentials and corporate profile metadata settings.</param>
        /// <param name="targetRole">The targeted assignment level role constraint parameter ("manager").</param>
        /// <returns>An HTTP response context layout showing transactional complete confirmation object payload properties.</returns>
        [HttpPost("register-staff")]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterStaffAsync([FromBody] AdminCreateCustomerRequest request, [FromQuery] string targetRole = "manager")
        {
            try
            {
                // Enforce strict business constraints regarding registration privileges
                if (targetRole != "manager")
                {
                    _logger.LogInformation("Only 'manager' role registrations are processed via this endpoint framework route pattern.");

                    return BadRequest(BaseResponse.Failure("Only 'manager' role registrations are allowed."));
                }

                // Delegate registration logic to business service layer architecture

                var targetDto = new RegisterCustomerDto
                {
                    Email = request.Email,
                    Password = request.Password,
                    UserCode = request.UserCode,
                    Address = request.Address,
                    Discount = request.Discount,
                    UserName = request.UserName
                };

                var response = await _customersService.RegisterCustomerAsync(targetDto, targetRole);

                // Handle validation or identity provider exceptions thrown below
                if (!response.IsSuccess)
                {
                    _logger.LogInformation("Error during internal staff member configuration parameters initialization: {Errors}", response.Errors);

                    return BadRequest(BaseResponse.Failure(response.Errors ?? ["Registration failed."]));
                }

                _logger.LogInformation("System access staff account baseline records populated successfully.");

                // Return structured unified schema matching BaseIdResponse standard output layout
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Critical component lifecycle pipeline disruption occurred inside staff sign-up wrapper. Trace: {Errors}", ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, BaseResponse.Failure(["Internal Server Error.", ex.Message]));
            }
        }
    }
}
