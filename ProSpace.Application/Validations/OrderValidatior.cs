using FluentValidation;
using ProSpace.Application.Properties;
using ProSpace.Contracts.DTO.Order;

namespace ProSpace.Application.Validations
{
    public class OrderValidatior : AbstractValidator<OrderDto>
    {
        public OrderValidatior()
        {
            RuleFor(x => x.CustomerId)
               .NotEmpty()
               .WithName(Resources.CustomerID);
        }
    }
}
