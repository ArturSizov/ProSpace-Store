using FluentValidation;
using ProSpace.Application.Properties;
using ProSpace.Contracts.DTO.Customer;

namespace ProSpace.Application.Validations.Customer
{
    public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerDto>
    {
        public RegisterCustomerValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithName(Resources.Password)
                .MinimumLength(6)
                .MaximumLength(100);

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithName(Resources.Name)
                .MinimumLength(2)
                .MaximumLength(100);

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0)
                .WithName(Resources.Discount)
                .LessThanOrEqualTo(100)
                .WithMessage(Resources.DiscountCannotExceed100);
        }
    }
}
