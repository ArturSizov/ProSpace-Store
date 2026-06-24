using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Mappers;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;
using ProSpace.Domain.Models;
using System.Security.Claims;

namespace ProSpace.Application.Services
{
    public class CustomersService : ICustomersService
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<CustomersService> _logger;

        /// <summary>
        /// User identity sevice
        /// </summary>
        private readonly IIdentityService _identityService;

        /// <summary>
        /// Customer validation service
        /// </summary>
        private readonly IValidator<CustomerDto> _validation;

        /// <summary>
        /// Register customer validation service
        /// </summary>
        private readonly IValidator<RegisterCustomerDto> _registerCustomerValidation;

        /// <summary>
        /// Unit of Work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Http context accessor
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        ///  Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="identityService"></param>
        /// <param name="userValidator"></param>
        /// <param name="customerValidator"></param>
        /// <param name="unitOfWork"></param>
        public CustomersService(ILogger<CustomersService> logger, 
            IIdentityService identityService,
            IValidator<CustomerDto> customerValidator,
            IValidator<RegisterCustomerDto> registerCustomerValidation,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _identityService = identityService;
            _validation = customerValidator;
            _registerCustomerValidation = registerCustomerValidation;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> RegisterCustomerAsync(RegisterCustomerDto dto, string role = "customer", CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating multi-tier entity registration pipeline execution sequence for email footprint: {Email}", dto.Email);

                var validate = await _registerCustomerValidation.ValidateAsync(dto, cancellationToken);

                if (!validate.IsValid)
                {
                    var validationErrorsCollection = validate.Errors.Select(e => e.ErrorMessage);

                    _logger.LogWarning("Registration request rejected downstream. Inbound payload properties failed validation rules: {Errors}",
                        string.Join("; ", validationErrorsCollection));

                    return BaseIdResponse.Failure(validationErrorsCollection);
                }

                var identityResult = await _identityService.CreateAccountAsync(dto.Email, dto.Password, role);

                if (!identityResult.IsSuccess)
                {
                    _logger.LogWarning("Multi-tier customer signup suspended. Security Provider rejected user creation for {Email}. Root: {Errors}",
                        dto.Email, string.Join(", ", identityResult.Errors ?? []));

                    return BaseIdResponse.Failure(identityResult.Errors ?? ["Identity service registration failure."]);
                }

                if (identityResult.Id == Guid.Empty)
                {
                    _logger.LogError("Critical infrastructure breakdown. Identity provider returned an empty tracking ID for validated email: {Email}", dto.Email);
                    return BaseIdResponse.Failure("Invalid internal membership account tracking identifier format generated.");
                }

                Guid appUserId = identityResult.Id;

                var currentUser = _httpContextAccessor.HttpContext?.User;
                var finalDiscount = dto.Discount;

                if (currentUser == null || (!currentUser.IsInRole("manager") && !currentUser.IsInRole("Manager")))
                {
                    if (dto.Discount > 0)
                    {
                        _logger.LogWarning("Unauthorized discount assignment intercepted for {Email}. Attempted: {Discount}%. Forced rollback to 0%.",
                            dto.Email, dto.Discount);
                    }

                    finalDiscount = 0;
                }

                var domainCustomer = new CustomerModel
                {
                    Id = Guid.NewGuid(),
                    AppUserId = appUserId,
                    Code = dto.UserCode,
                    Name = dto.UserName,
                    Address = dto.Address,
                    Discount = finalDiscount
                };

                await _unitOfWork.CustomersRepository.CreateAsync(domainCustomer, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Database unit of work flush sequence failed. Compensating transactional actions triggered to wipe security trace for {Email}.", dto.Email);

                    await _identityService.DeleteAccountAsync(appUserId);

                    return BaseIdResponse.Failure("Failed to synchronize customer profile storage matrices down to persistent data stores.");
                }

                _logger.LogInformation("Multi-tier customer allocation finalized successfully. Profile Node: {Name} (Id: {Id}) mapped to Auth Node: {AuthId}",
                    domainCustomer.Name, domainCustomer.Id, appUserId);

                return BaseIdResponse.Success(domainCustomer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical unhandled lifecycle crash disrupted the multi-tier registration processing pipelines context for target: {Email}", dto.Email);
                return BaseIdResponse.Failure("A critical server exception tracking error occurred during customer workflow execution loops.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiating comprehensive customer extraction sequence for tracking key target: {Id}", id);

                var customer = await _unitOfWork.CustomersRepository.ReadAsync(id, cancellationToken);

                if (customer == null)
                {
                    _logger.LogWarning("Extraction operation suspended. Customer record with ID {Id} was not located.", id);
                    return BaseIdResponse.Failure($"Customer with ID {id} not found inside the database system registers.");
                }

                await _unitOfWork.CustomersRepository.DeleteAsync(id, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Database unit of work flush sequence failed. Cascade execution halted for customer {Id}.", id);
                    return BaseIdResponse.Failure("Failed to commit and complete customer profile deletion inside the core database.");
                }

                var identityResult = await _identityService.DeleteAccountAsync(customer.AppUserId);

                if (!identityResult.IsSuccess)
                {
                    _logger.LogError("Database profile erased successfully, but Identity account cleanup failed for target {Id}. Reason: {Errors}",
                        id, string.Join(", ", identityResult.Errors ?? []));

                    return BaseIdResponse.Failure(identityResult.Errors ?? ["Identity credentials cleanup task failure."]);
                }

                _logger.LogInformation("Multi-tier deletion workflow finalized successfully. Customer {Id} and Identity trace cleared completely.", id);
                return BaseIdResponse.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical unhandled exception disrupted the multi-tier customer profile deletion pipeline for ID: {Id}", id);
                return BaseIdResponse.Failure("A critical server exception tracking error occurred during customer profile extraction.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<CustomerDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BaseResponse<CustomerDto>.Failure("Email cannot be empty.");

                var customer = await _unitOfWork.CustomersRepository.GetByEmailAsync(email, cancellationToken);

                if (customer == null)
                {
                    _logger.LogWarning("Buyer with email '{Email}' not found.", email);
                    return BaseResponse<CustomerDto>.Failure($"Buyer with email '{email}' not found.");
                }

                var customerDto = customer.ToDto();

                return BaseResponse<CustomerDto>.Success(customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for buyer by Email: {Email}", email);

                return BaseResponse<CustomerDto>.Failure("An internal server error occurred while searching for the buyer.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<IEnumerable<CustomerDto>>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var domainCustomers = await _unitOfWork.CustomersRepository.ReadAllAsync(cancellationToken);

                if (domainCustomers == null || !domainCustomers.Any())
                {
                    _logger.LogInformation("No customers found in the database.");
                    return BaseResponse<IEnumerable<CustomerDto>>.Success([]);
                }

                _logger.LogInformation("Total users found: {Count}", domainCustomers.Count());

                var dtos = domainCustomers.Select(x => x.ToDto()).ToArray();

                return BaseResponse<IEnumerable<CustomerDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading list of all customers.");
                return BaseResponse<IEnumerable<CustomerDto>>.Failure("Failed to load customer list.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<CustomerDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = await _unitOfWork.CustomersRepository.ReadAsync(id, cancellationToken);

                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID {Id} not found.", id);

                    return BaseResponse<CustomerDto>.Failure($"Customer with ID {id} not found.");
                }

                var customerDto = customer.ToDto();

                _logger.LogInformation("Customer loaded successfully: {CustomerName}", customer.Name);

                return BaseResponse<CustomerDto>.Success(customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for buyer by Id: {Id}", id);

                return BaseResponse<CustomerDto>.Failure("Unable to search for buyer due to an internal error.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<CustomerDto>> UpdateAsync(CustomerDto customer, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing business rules validation tracking matrix for customer update: {Id}", customer.Id);

                var validate = await _validation.ValidateAsync(customer, cancellationToken);

                if (!validate.IsValid)
                {
                    var validationErrors = validate.Errors.Select(e => e.ErrorMessage);
                    _logger.LogWarning("Validation constraint violation triggered when updating customer {Id}: {Errors}",
                        customer.Id, string.Join("; ", validationErrors));

                    return BaseResponse<CustomerDto>.Failure(validationErrors);
                }

                var existingCustomer = await _unitOfWork.CustomersRepository.ReadAsync(customer.Id, cancellationToken);

                if (existingCustomer == null)
                {
                    _logger.LogWarning("Data modification halted. Customer with tracking ID {Id} was not located in stores.", customer.Id);
                    return BaseResponse<CustomerDto>.Failure($"Customer with ID {customer.Id} not found.");
                }

                var currentUser = _httpContextAccessor.HttpContext?.User;

                if (currentUser != null && !currentUser.IsInRole("manager") && !currentUser.IsInRole("Manager"))
                {
                    var nameIdentifierClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!Guid.TryParse(nameIdentifierClaim, out var authenticatedUserId))
                    {
                        _logger.LogError("Security system breakdown: Unable to resolve a valid Guid from user NameIdentifier claim.");
                        return BaseResponse<CustomerDto>.Failure("Access denied. Invalid user identity context.");
                    }

                    if (authenticatedUserId != existingCustomer.AppUserId)
                    {
                        _logger.LogCritical("Security breach attempt blocked! Authenticated User {AuthId} tried to modify Customer profile {CustomerId} (Owned by: {OwnerId})",
                            authenticatedUserId, customer.Id, existingCustomer.AppUserId);

                        return BaseResponse<CustomerDto>.Failure("Access denied. You are not authorized to modify this customer profile configuration.");
                    }

                    if (customer.Discount != existingCustomer.Discount)
                    {
                        _logger.LogWarning("Privilege escalation blocked. Customer account {Id} attempted to adjust discount parameter rates from {Old} to {New}",
                            customer.Id, existingCustomer.Discount, customer.Discount);

                        return BaseResponse<CustomerDto>.Failure("Authorization rejected. Standard customer accounts are blocked from modifying assignment discount thresholds.");
                    }
                }

                existingCustomer.Name = customer.Name;
                existingCustomer.Address = customer.Address;
                existingCustomer.Discount = customer.Discount;
                existingCustomer.Code = customer.Code;

                await _unitOfWork.CustomersRepository.UpdateAsync(existingCustomer, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Database unit of work sync failure encountered while finalizing update tasks for customer: {CustomerName}", customer.Name);
                    return BaseResponse<CustomerDto>.Failure("Failed to sync customer profile structural parameters into persistent stores.");
                }

                _logger.LogInformation("Customer data profile state modification completed successfully for record: {CustomerName}", existingCustomer.Name);

                var updatedDto = existingCustomer.ToDto();
                return BaseResponse<CustomerDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled execution lifecycle exception disrupted data synchronization tasks for customer tracking ID: {Id}", customer.Id);
                return BaseResponse<CustomerDto>.Failure("An internal service error occurred while processing the customer profile update transaction.");
            }
        }
    }
}
