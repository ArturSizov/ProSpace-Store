using FluentValidation;
using ProSpace.Application.Properties;
using ProSpace.Contracts.DTO;
using System.Text.RegularExpressions;

namespace ProSpace.Application.Validations
{
    public class CustomerValidator : AbstractValidator<CustomerDto>
    {
        private static readonly Regex _regex = new(@"^\d{4}-\d{4}$");
        public CustomerValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(Resources.BuyerIDRequiredToUpdate);


            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(i => i.Code)
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
