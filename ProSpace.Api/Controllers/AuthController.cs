using Microsoft.AspNetCore.Mvc;
using ProSpace.Application.Common.Models;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Contracts.Contracts.Request;
using ProSpace.Contracts.Contracts.Response;
using ProSpace.Contracts.Responses;

namespace ProSpace.Api.Controllers
{
    /// <summary>
    /// Provides HTTP endpoints for user authentication, token generation, and identity management.
    /// </summary>
    public class AuthController : BaseApiController
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// User identity service
        /// </summary>
        private readonly IIdentityService _identityService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        public AuthController(ILogger<AuthController> logger, IIdentityService identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }

        /// <summary>
        /// Authenticates a user with email and password, returning a secure JWT token upon success.
        /// </summary>
        /// <param name="request">The login request model containing the user's registered email and password.</param>
        /// <returns>A unified response wrapper containing the access token and user identity metrics.</returns>
        /// <response code="200">The credentials are valid, and the authorization token was generated successfully.</response>
        /// <response code="400">The login request fields failed validation constraints.</response>
        /// <response code="401">Invalid email or password provided.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _identityService.LoginAsync(request.Email, request.Password);

            _logger.LogInformation("User {Email} logged in successfully.", request.Email);
            return ProcessResponse(result);
        }
    }
}
