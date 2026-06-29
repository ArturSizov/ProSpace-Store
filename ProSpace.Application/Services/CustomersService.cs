using FluentValidation;
using Microsoft.Extensions.Logging;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Mappers;
using ProSpace.Contracts.DTO.Customer;
using ProSpace.Contracts.Responses;
using ProSpace.Domain.Models;

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
        /// Security service
        /// </summary>
        private readonly ISecurityService _securityService;

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
            ISecurityService securityService)
        {
            _logger = logger;
            _identityService = identityService;
            _validation = customerValidator;
            _registerCustomerValidation = registerCustomerValidation;
            _unitOfWork = unitOfWork;
            _securityService = securityService;
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

                string finalRole = role;

                if (!_securityService.IsManager())
                {
                    if (role != "customer")
                    {
                        _logger.LogWarning("Unauthorized role escalation attempt intercepted for {Email}. Attempted role: {Role}. Overriding to 'customer'.",
                            dto.Email, role);
                    }
                    finalRole = "customer";
                }

                var finalDiscount = dto.Discount;
                if (!_securityService.IsManager())
                {
                    if (dto.Discount > 0)
                    {
                        _logger.LogWarning("Unauthorized discount assignment intercepted for {Email}. Attempted: {Discount}%. Forced rollback to 0%.",
                            dto.Email, dto.Discount);
                    }
                    finalDiscount = 0;
                }

                var identityResult = await _identityService.CreateAccountAsync(dto.Email, dto.Password, finalRole);

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

                var userCode = await _unitOfWork.CustomersRepository.GenerateNextCustomerCodeAsync(cancellationToken);

                var domainCustomer = new CustomerModel
                {
                    Id = Guid.NewGuid(),
                    AppUserId = appUserId,
                    Code = userCode,
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
                _logger.LogInformation("Initiating comprehensive customer deletion sequence for tracking key target: {Id}", id);

                var customer = await _unitOfWork.CustomersRepository.ReadAsync(id, cancellationToken);

                if (customer == null)
                {
                    _logger.LogWarning("Deletion operation suspended. Customer record with ID {Id} was not located.", id);
                    return BaseIdResponse.Failure($"Customer with ID {id} not found inside the database system registers.");
                }
       
                if (!_securityService.IsManager())
                {
                    _logger.LogInformation("Standard customer account verified. Enforcing Soft-Delete boundaries for ID: {Id}", id);

                    var (isSuccess, error) = await _securityService.ValidateCustomerAccessAsync(id, cancellationToken);
                    if (!isSuccess)
                    {
                        _logger.LogCritical("Security violation! Unauthorized user tried to execute deletion pipeline for Customer ID: {Id}", id);
                        return BaseIdResponse.Failure(error);
                    }

                    var blockResult = await _identityService.BlockAccountAsync(customer.AppUserId);
                    if (!blockResult.IsSuccess)
                    {
                        _logger.LogWarning("Customer deactivation suspended. Identity provider rejected account lockout for {Id}. Reason: {Errors}",
                            id, string.Join(", ", blockResult.Errors ?? []));
                        return BaseIdResponse.Failure(blockResult.Errors ?? ["Identity credentials lockout task failure."]);
                    }

                    await _unitOfWork.CustomersRepository.SoftDeleteAsync(id, cancellationToken);

                    if (!await _unitOfWork.CompleteAsync(cancellationToken))
                    {
                        _logger.LogError("Database unit of work flush sequence failed during customer soft-deletion for ID: {Id}", id);
                        return BaseIdResponse.Failure("Failed to complete the customer deactivation inside persistent stores.");
                    }

                    _logger.LogInformation("Soft-delete workflow finalized successfully. Customer profile {Id} deactivated and locked out.", id);
                    return BaseIdResponse.Success(id);
                }

                _logger.LogInformation("Manager privileges verified. Evaluating Hard-Delete eligibility for Customer ID: {Id}", id);

                var customerOrders = await _unitOfWork.OrdersRepository.GetOrdersByCustomerIdAsync(id, cancellationToken);
                if (customerOrders != null && customerOrders.Any())
                {
                    _logger.LogWarning("Hard-deletion rejected by system rules. Customer {Id} possesses active historical records inside orders registries.", id);
                    return BaseIdResponse.Failure("Cannot hard-delete a customer with existing orders. Please instruct the system to deactivate or archive the profile instead.");
                }

                var identityResult = await _identityService.DeleteAccountAsync(customer.AppUserId);
                if (!identityResult.IsSuccess)
                {
                    _logger.LogWarning("Administrative hard-deletion suspended. Identity Service rejected account cleanup for target {Id}. Reason: {Errors}",
                        id, string.Join(", ", identityResult.Errors ?? []));

                    return BaseIdResponse.Failure(identityResult.Errors ?? ["Identity credentials cleanup task failure."]);
                }

                await _unitOfWork.CustomersRepository.DeleteAsync(id, cancellationToken);

                if (!await _unitOfWork.CompleteAsync(cancellationToken))
                {
                    _logger.LogError("Database unit of work flush sequence failed. Administrative hard-deletion halted for customer {Id}.", id);
                    return BaseIdResponse.Failure("Failed to commit and complete customer profile hard-deletion inside the core database.");
                }

                _logger.LogInformation("Administrative hard-deletion workflow finalized successfully. Customer {Id} and Identity trace cleared completely from registries.", id);
                return BaseIdResponse.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical unhandled exception disrupted the multi-tier customer profile deletion pipeline for ID: {Id}", id);
                return BaseIdResponse.Failure("A critical server exception tracking error occurred during customer profile extraction operations.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<CustomerDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiate customer search by email: {Email}", email);

                if (string.IsNullOrWhiteSpace(email))
                    return BaseResponse<CustomerDto>.Failure("Email cannot be empty.");

                _logger.LogInformation("The process of reading the Customer by Email has begun.");

                if (_securityService.IsManager())
                {
                    var fullCustomer = await _unitOfWork.CustomersRepository.GetByEmailAsync(email, cancellationToken);

                    if (fullCustomer == null)
                    {
                        _logger.LogWarning("Customer with email {Email} not found.", email);
                        return BaseResponse<CustomerDto>.Failure($"Customer with email {email} not found.");
                    }

                    return BaseResponse<CustomerDto>.Success(fullCustomer.ToDto());
                }

                var authenticatedUserEmail = _securityService.GetCurrentUserEmail();

                if (string.IsNullOrEmpty(authenticatedUserEmail) || !string.Equals(authenticatedUserEmail, email, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogCritical("Security breach attempt! Unauthorized user tried to access customer profile data using Email: {AttemptedEmail}", email);
                    return BaseResponse<CustomerDto>.Failure($"Customer with email {email} not found."); 
                }

                var authenticatedCustomerId = await _securityService.GetCurrentCustomerIdAsync(cancellationToken);

                if (authenticatedCustomerId == null || authenticatedCustomerId == Guid.Empty)
                {
                    _logger.LogWarning("Customer compilation terminated. Active user does not possess an initialized Customer profile context mapping.");
                    return BaseResponse<CustomerDto>.Failure("Access denied. Customer profile not registered.");
                }

                _logger.LogInformation("Standard customer account verified. Executing safe isolated database query for Customer ID: {CustomerId}", authenticatedCustomerId.Value);

                var customer = await _unitOfWork.CustomersRepository.ReadAsync(authenticatedCustomerId.Value, cancellationToken);

                if (customer == null)
                    return BaseResponse<CustomerDto>.Failure($"Customer with email {email} not found.");


                return BaseResponse<CustomerDto>.Success(customer.ToDto());
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
                _logger.LogInformation("Initiate retrieval of all customers from the data layer.");

                if (!_securityService.IsManager())
                {
                    _logger.LogCritical("Security violation alert! An unauthorized standard user tried to bypass API parameters to extract the complete global customer directory.");

                    return BaseResponse<IEnumerable<CustomerDto>>.Failure("Access denied. Only managers possess rights to read global directories.");
                }

                var domainCustomers = await _unitOfWork.CustomersRepository.ReadAllAsync(cancellationToken);

                var customersList = domainCustomers?.ToList() ?? [];

                if (customersList.Count == 0)
                {
                    _logger.LogInformation("No active customers found in the database system registers.");
                    return BaseResponse<IEnumerable<CustomerDto>>.Success([]);
                }

                _logger.LogInformation("Total users found and successfully loaded: {Count}", customersList.Count);

                var dtos = customersList.Select(x => x.ToDto()).ToList();

                return BaseResponse<IEnumerable<CustomerDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading list of all customers due to a structural backend crash.");
                return BaseResponse<IEnumerable<CustomerDto>>.Failure("Failed to load customer list.");
            }
        }

        /// <inheritdoc/>
        public async Task<BaseResponse<CustomerDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initiate customer search by id: {Id}", id);

                var customer = await _unitOfWork.CustomersRepository.ReadAsync(id, cancellationToken);

                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID {Id} not found.", id);

                    return BaseResponse<CustomerDto>.Failure($"Customer with ID {id} not found.");
                }

                var (isSuccess, error) = await _securityService.ValidateCustomerAccessAsync(id, cancellationToken);

                if (!isSuccess)
                {
                    _logger.LogCritical("Security enforcement blocked unauthorized access attempt to Customer ID: {Id}", id);

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
                Guid targetCustomerId = customer.Id;

                if (targetCustomerId == Guid.Empty)
                {
                    var authenticatedCustomerId = await _securityService.GetCurrentCustomerIdAsync(cancellationToken);

                    if (authenticatedCustomerId == null || authenticatedCustomerId == Guid.Empty)
                    {
                        _logger.LogWarning("Customer compilation terminated. Active user context mapping not found.");
                        return BaseResponse<CustomerDto>.Failure("Access denied. Customer profile not registered.");
                    }

                    targetCustomerId = authenticatedCustomerId.Value;

                    customer.Id = targetCustomerId;
                }

                _logger.LogInformation("Processing business rules validation tracking matrix for customer update: {Id}", targetCustomerId);

                var validate = await _validation.ValidateAsync(customer, cancellationToken);

                if (!validate.IsValid)
                {
                    var validationErrors = validate.Errors.Select(e => e.ErrorMessage);
                    _logger.LogWarning("Validation constraint violation triggered when updating customer {Id}: {Errors}",
                        targetCustomerId, string.Join("; ", validationErrors));

                    return BaseResponse<CustomerDto>.Failure(validationErrors);
                }

                var existingCustomer = await _unitOfWork.CustomersRepository.ReadAsync(targetCustomerId, cancellationToken);

                if (existingCustomer == null)
                {
                    _logger.LogWarning("Data modification halted. Customer with tracking ID {Id} was not located in stores.", targetCustomerId);
                    return BaseResponse<CustomerDto>.Failure($"Customer with ID {targetCustomerId} not found.");
                }

                var (isSuccess, error) = await _securityService.ValidateCustomerAccessAsync(targetCustomerId, cancellationToken);

                if (!isSuccess)
                {
                    _logger.LogCritical("Security enforcement blocked unauthorized update attempt to Customer ID: {CustomerId}", targetCustomerId);
                    return BaseResponse<CustomerDto>.Failure($"Customer with ID {targetCustomerId} not found.");
                }

                existingCustomer.Name = customer.Name;
                existingCustomer.Address = customer.Address;

                if (_securityService.IsManager())
                {
                    _logger.LogInformation("Manager administrative override detected for customer {Id}. Applying restricted fields (Discount and Code).", targetCustomerId);

                    if (existingCustomer.Code != customer.Code)
                    {
                        var isCodeTaken = await _unitOfWork.CustomersRepository.IsCodeAssignedToAnotherCustomerAsync(customer.Code, targetCustomerId, cancellationToken);

                        if (isCodeTaken)
                        {
                            _logger.LogWarning("Administrative override rejected. Customer code {Code} is already assigned to another profile.", customer.Code);
                            return BaseResponse<CustomerDto>.Failure($"The customer code '{customer.Code}' is already in use by another account.");
                        }

                        existingCustomer.Code = customer.Code;
                    }

                    existingCustomer.Discount = customer.Discount;
                }

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
