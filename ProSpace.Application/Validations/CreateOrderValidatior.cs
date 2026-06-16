using FluentValidation;
using ProSpace.Contracts.DTO;

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
