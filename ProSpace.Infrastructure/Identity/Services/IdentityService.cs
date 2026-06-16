using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Common.Interfaces;
using ProSpace.Application.Common.Models;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Contracts.Contracts.Request;
using ProSpace.Contracts.Contracts.Response;
using ProSpace.Contracts.Responses;
using ProSpace.Infrastructure.Entites.Users;

namespace ProSpace.Infrastructure.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        /// <summary>
        /// Asp user manager
        /// </summary>
        private readonly UserManager<AppUser> _userManager;

        /// <summary>
        /// User role manager
        /// </summary>
        private readonly RoleManager<AppRole> _roleManager;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<IdentityService> _logger;

        /// <summary>
        /// Token service
        /// </summary>
        private readonly ITokenService _tokenService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="userManager"></param>
        /// <param name="roleManager"></param>
        /// <param name="tokenService"></param>
        public IdentityService(ILogger<IdentityService> logger, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
             ITokenService tokenService)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> CreateAccountAsync(string email, string password, string role)
        {
            try
            {
                var roleExists = await _roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                    _logger.LogError("Attempted to register with a non-existent role: {Role}", role);
                    return BaseIdResponse.Failure([$"Role '{role}' does not exist in the system."]);
                }

                var user = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BaseIdResponse.Failure(errors);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, role);

                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description);
                    return BaseIdResponse.Failure(errors);
                }

                _logger.LogInformation("Account {Email} created with ID {UserId}", email, user.Id);

                return BaseIdResponse.Success(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create account: {Message}", ex.Message);
                return BaseIdResponse.Failure([$"Failed to create account: {ex.Message}"]);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CheckPasswordAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Initiating runtime password validation trace cycle for email target: {Email}", email);

                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("Authentication sequence suspended downstream. Requested identity footprint target {Email} is not registered.", email);
                    return false;
                }

                bool isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Authentication verification rejected. Invalid credentials provided for identity reference target: {Email}", email);
                    return false;
                }

                _logger.LogInformation("Authentication validation completed successfully. Access token generation cleared for user target: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A critical system exception disrupted the cryptographic password verification flow loops for email target: {Email}", email);
                return false;
            }
        }


        /// <inheritdoc/>
        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching security role identities for user account ID: {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("Role lookup suspended. User account {UserId} does not exist in the store.", userId);
                    return [];
                }

                var roles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation("Successfully retrieved system roles for user {UserId}. Active roles: [{Roles}]",
                    userId, string.Join(", ", roles));

                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A critical infrastructure error occurred while resolving role states for user: {UserId}", userId);

                return [];
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<LoginResponse>> LoginAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Attempting login verification authentication parameters sequence workflow loop for email: {Email}", email);

                var user = await _userManager.FindByEmailAsync(email);

                if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                {
                    _logger.LogWarning("Authentication flow blocked downstream. Incorrect credentials provided for signature target identifier.");

                    return BaseResponse<LoginResponse>.Failure("Incorrect email or password.");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var (token, expiration) = _tokenService.GenerateJwtToken(user.Id.ToString(), user.UserName ?? string.Empty, roles);

                var loginResponsePayload = new LoginResponse
                {
                    Token = token,
                    UserId = user.Id,
                    Roles = roles,
                    Expiration = expiration
                };

                _logger.LogInformation("The identity verification sequence concluded successfully. User account ID: {UserId} authorized.", user.Id);

                return BaseResponse<LoginResponse>.Success(loginResponsePayload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A critical runtime system error disrupted the user lifecycle authentication processing pipeline pipeline execution blocks.");

                return BaseResponse<LoginResponse>.Failure("A critical server exception tracking error occurred during user authentication workflow operations.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> DeleteAccountAsync(Guid appUserId)
        {
            try
            {
                _logger.LogInformation("Initiating core identity authentication provider account deletion cascade loop for tracker key: {Id}", appUserId);

                var user = await _userManager.FindByIdAsync(appUserId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("Account extraction operation suspended. User entity credential trace {Id} was not located.", appUserId);

                    return BaseIdResponse.Failure($"User with account credential link ID {appUserId} not found inside identity registry context.");
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    var systemErrorsCollection = result.Errors.Select(e => e.Description);

                    _logger.LogError("The database internal user identity management engine rejected the deletion query cascade. Reason traces: {Errors}",
                        string.Join(", ", systemErrorsCollection));

                    return BaseIdResponse.Failure(systemErrorsCollection);
                }

                _logger.LogInformation("Core user authorization baseline profiles wiped completely. Identity target ID: {Id} deleted.", appUserId);

                return BaseIdResponse.Success(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An unhandled execution crash disrupted the internal membership authorization persistence transaction thread loops.");

                return BaseIdResponse.Failure("A critical identity manager crash occurred while executing the request account deletion sequence.");
            }
        }
    }
}
