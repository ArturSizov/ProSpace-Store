using FluentValidation;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Properties;
using ProSpace.Contracts.DTO.Customer;
using System.Text.RegularExpressions;

namespace ProSpace.Application.Validations.Customer
{
    public class CustomerValidator : AbstractValidator<CustomerDto>
    {
        private static readonly Regex _regex = new(@"^\d{4}-\d{4}$");

        public CustomerValidator(ISecurityService securityService)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(Resources.BuyerIDRequiredToUpdate);

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithName(Resources.Name)
                .MinimumLength(2)
                .MaximumLength(100);

            RuleFor(i => i.Code)
                 .NotEmpty()
                 .WithName(Resources.UserCode)
                 .Matches(_regex)
                 .WithFormat(Resources.CodeFormatError, "2222-2222")
                 .When(_ => securityService.IsManager()); // Enabled only for the manager

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0)
                .WithName(Resources.Discount)
                .LessThanOrEqualTo(100)
                .WithMessage(Resources.DiscountCannotExceed100)
                .When(_ => securityService.IsManager()); // Enabled only for the manager
        }
    }
}
