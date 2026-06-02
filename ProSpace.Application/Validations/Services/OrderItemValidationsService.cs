using FluentValidation;
using ProSpace.Domain.Interfaces.Validations;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Validations.Services
{
    public class OrderItemValidationsService : IValidationProvider<OrderItemModel>
    {
        /// <summary>
        /// Order item validator
        /// </summary>
        private readonly IValidator<OrderItemModel> _validator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="validator"></param>
        public OrderItemValidationsService(IValidator<OrderItemModel> validator)
        {
            _validator = validator;
        }

        /// <inheritdoc/>
        public async Task<(bool, IDictionary<string, string[]>?)> ValidateAsync(OrderItemModel orderItem)
        {
            var validate = await _validator.ValidateAsync(orderItem ?? throw new ArgumentNullException(nameof(orderItem)));

            return (validate.IsValid, validate.ToDictionary());
        }
    }
}
