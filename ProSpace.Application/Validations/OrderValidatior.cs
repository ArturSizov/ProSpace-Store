using FluentValidation;
using ProSpace.Contracts.DTO;

namespace ProSpace.Application.Validations
{
    public class OrderValidatior : AbstractValidator<OrderDto>
    {
        public OrderValidatior()
        {
            RuleFor(x => x.CustomerId)
               .NotEmpty();

            RuleFor(x => x.OrderDate)
               .NotEmpty();
        }
    }
}
