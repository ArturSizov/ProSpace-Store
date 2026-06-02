using FluentValidation;
using ProSpace.Domain.Interfaces.Validations;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Validations.Services
{
    public class OrderValidationsService : IValidationProvider<OrderModel>
    {
        /// <summary>
        /// Order validator
        /// </summary>
        private readonly IValidator<OrderModel> _validator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="validator"></param>
        public OrderValidationsService(IValidator<OrderModel> validator)
        {
            _validator = validator;
        }

        /// <inheritdoc/>
        public async Task<(bool, IDictionary<string, string[]>?)> ValidateAsync(OrderModel order)
        {
            var validate = await _validator.ValidateAsync(order ?? throw new ArgumentNullException(nameof(order)));

            return (validate.IsValid, validate.ToDictionary());
        }
    }
}
