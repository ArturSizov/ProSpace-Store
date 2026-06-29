using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using System.Security.Claims;

namespace ProSpace.Infrastructure.Services
{
    public class SecurityService : ISecurityService
    {
        /// <summary>
        /// Unit of work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<SecurityService> _logger;

        /// <summary>
        /// Http context accessor
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        /// <param name="httpContextAccessor"></param>
        public SecurityService(IUnitOfWork unitOfWork, ILogger<SecurityService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public async Task<(bool IsSuccess, string Error)> ValidateCustomerAccessAsync(Guid customerId, CancellationToken ct = default)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser != null && (currentUser.IsInRole("manager") || currentUser.IsInRole("Manager")))
                return (true, string.Empty);

            var nameIdentifierClaim = currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(nameIdentifierClaim, out var authenticatedUserId))
            {
                _logger.LogError("Security system breakdown: Unable to resolve a valid Guid from user NameIdentifier claim context.");
                return (false, "Access denied. Invalid user identity.");
            }

            var customerProfile = await _unitOfWork.CustomersRepository.ReadAsync(customerId, ct);

            if (customerProfile == null)
            {
                _logger.LogWarning("Access evaluation suspended. Customer record {Id} not found.", customerId);
                return (false, "Resource not found.");
            }

            if (authenticatedUserId != customerProfile.AppUserId)
            {
                _logger.LogCritical("Security breach attempt blocked! User {AuthId} tried to access data owned by Customer {CustomerId}.",
                    authenticatedUserId, customerId);

                return (false, "Resource not found.");
            }

            return (true, string.Empty);
        }

        /// <inheritdoc/>
        public bool IsManager()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user != null && (user.IsInRole("manager") || user.IsInRole("Manager"));
        }

        /// <inheritdoc/>
        public async Task<Guid?> GetCurrentCustomerIdAsync(CancellationToken cancellationToken = default)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            var nameIdentifierClaim = currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(nameIdentifierClaim, out var authenticatedUserId))
                return null;

            var customerProfile = await _unitOfWork.CustomersRepository.GetByAppUserIdAsync(authenticatedUserId, cancellationToken);

            return customerProfile?.Id;
        }

        /// <inheritdoc/>
        public string? GetCurrentUserEmail()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            return currentUser?.FindFirst(ClaimTypes.Email)?.Value
                ?? currentUser?.FindFirst("email")?.Value;
        }
    }
}
