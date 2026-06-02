using FluentValidation;
using ProSpace.Domain.Interfaces.Validations;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Validations.Services
{
    public class CustomerValidationsService : IValidationProvider<CustomerModel>
    {
        /// <summary>
        /// Customer validator
        /// </summary>
        private readonly IValidator<CustomerModel> _validator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="validator"></param>
        public CustomerValidationsService(IValidator<CustomerModel> validator)
        {
           _validator = validator;
        }

        /// <inheritdoc/>
        public async Task<(bool, IDictionary<string, string[]>?)> ValidateAsync(CustomerModel customer)
        {
            var validate = await _validator.ValidateAsync(customer ?? throw new ArgumentNullException(nameof(customer)));

            return (validate.IsValid, validate.ToDictionary());
        }
    }
}
