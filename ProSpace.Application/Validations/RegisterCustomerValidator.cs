using FluentValidation;
using ProSpace.Application.Properties;
using ProSpace.Contracts.DTO;
using System.Text.RegularExpressions;

namespace ProSpace.Application.Validations
{
    public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerDto>
    {
        private static readonly Regex _regex = new(@"^\d{4}-\d{4}$");

        public RegisterCustomerValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(100);

            RuleFor(x => x.UserName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(i => i.UserCode)
                 .NotEmpty()
                 .WithName(Resources.UserCode)
                 .Matches(_regex)
                 .WithFormat(Resources.CodeFormatError, "2222-2222");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0)
                .WithName(Resources.Discount)
                .LessThanOrEqualTo(100)
                .WithMessage(Resources.DiscountCannotExceed100);
        }
    }
}
