using FluentValidation;
using FluentValidation.Results;
using ProSpace.Domain.Interfaces.Validations;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Validations.Services
{
    public class ItemValidationsService : IValidationProvider<ItemModel>
    {
        /// <summary>
        /// Item validator
        /// </summary>
        private readonly IValidator<ItemModel> _validator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="validator"></param>
        public ItemValidationsService(IValidator<ItemModel> validator)
        {
            _validator = validator;
        }

        /// <inheritdoc/>
        public async Task<(bool, IDictionary<string, string[]>?)> ValidateAsync(ItemModel item)
        {
            var validate = await _validator.ValidateAsync(item ?? throw new ArgumentNullException(nameof(item)));

            return (validate.IsValid, validate.ToDictionary());
        }
    }
}
