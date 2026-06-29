using FluentValidation;
using ProSpace.Application.Properties;
using ProSpace.Contracts.DTO.OrderItem;

namespace ProSpace.Application.Validations.OrderItem
{
    public class CreateOrderItemValidatior : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemValidatior()
        {
            RuleFor(x => x.ItemsCount)
               .GreaterThan(0)
               .WithName(Resources.ItemsCount)
               .WithName(Resources.ItemsCount);
        }
    }
}
