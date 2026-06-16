using FluentValidation;
using ProSpace.Contracts.DTO;

namespace ProSpace.Application.Validations
{
    public class CreateOrderItemValidatior : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemValidatior()
        {
            RuleFor(x => x.OrderId)
               .NotEmpty();

            RuleFor(x => x.ItemId)
               .NotEmpty();

            RuleFor(x => x.ItemsCount)
               .NotEmpty();

            RuleFor(x => x.ItemPrice)
               .NotEmpty();
        }
    }
}
