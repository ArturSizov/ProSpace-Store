using FluentValidation;
using ProSpace.Contracts.DTO;

namespace ProSpace.Application.Validations
{
    public class OrderItemValidatior : AbstractValidator<OrderItemDto>
    {
        public OrderItemValidatior()
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
