using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<(bool IsSuccess, string Error)> ValidateOrderOwnershipAsync(Guid customerId)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser != null && (currentUser.IsInRole("manager") || currentUser.IsInRole("Manager")))
                return (true, string.Empty);

            var nameIdentifierClaim = currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(nameIdentifierClaim, out var authenticatedUserId))
            {
                _logger.LogError("Security system breakdown: Unable to resolve a valid Guid from user NameIdentifier claim context.");
                return (false, "Access denied. Invalid user identity configuration parameters.");
            }

            var customerProfile = await _unitOfWork.CustomersRepository.ReadAsync(customerId);

            if (customerProfile == null)
            {
                _logger.LogWarning("Access evaluation suspended. Parent Customer record {Id} was not located in database registers.", customerId);
                return (false, "Associated customer account data profile was not found.");
            }

            if (authenticatedUserId != customerProfile.AppUserId)
            {
                _logger.LogCritical("Security breach attempt blocked! Authenticated standard user {AuthId} tried to access data owned by Customer profile {CustomerId} (Owned by user: {OwnerId}).",
                    authenticatedUserId, customerId, customerProfile.AppUserId);

                return (false, "Access denied. You do not possess structural permissions to view or modify this resource.");
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
    }
}
