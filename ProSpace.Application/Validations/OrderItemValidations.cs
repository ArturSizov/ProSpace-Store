using FluentValidation;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Validations
{
    public class OrderItemValidations : AbstractValidator<OrderItemModel>
    {
        public OrderItemValidations()
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
