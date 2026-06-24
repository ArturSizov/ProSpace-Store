using FluentValidation;
using ProSpace.Contracts.DTO.Order;

namespace ProSpace.Application.Validations
{
    public class CreateOrderValidatior : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderValidatior()
        {
            RuleFor(x => x.CustomerId)
               .NotEmpty();

            RuleFor(x => x.OrderDate)
               .NotEmpty();
        }
    }
}
