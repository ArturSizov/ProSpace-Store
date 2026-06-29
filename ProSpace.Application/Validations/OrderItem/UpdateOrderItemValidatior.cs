using FluentValidation;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Properties;
using ProSpace.Contracts.DTO.OrderItem;

namespace ProSpace.Application.Validations
{
    public class UpdateOrderItemValidator : AbstractValidator<UpdateOrderItemDto>
    {
        public UpdateOrderItemValidator(ISecurityService securityService)
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.ItemId)
                .NotEmpty();

            RuleFor(x => x.ItemsCount)
                .GreaterThan(0)
                .WithName(Resources.ItemsCount);

            RuleFor(x => x.ItemPrice)
                .GreaterThanOrEqualTo(0)
                .WithName(Resources.ItemPrice)
                .WithMessage(Resources.AdminPrice)
                .When(_ => securityService.IsManager()); // Triggers only for the manager.

            RuleFor(x => x.ItemPrice)
                .Equal(0)
                .WithName(Resources.ItemPrice)
                .WithMessage(Resources.StandartCustomerPrice)
                .When(_ => !securityService.IsManager()); // Triggers only for a standard client.
        }
    }
}
