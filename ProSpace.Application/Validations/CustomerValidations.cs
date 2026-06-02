using FluentValidation;
using ProSpace.Application.Properties;
using ProSpace.Domain.Models;
using System.Text.RegularExpressions;

namespace ProSpace.Application.Validations
{
    public class CustomerValidations : AbstractValidator<CustomerModel>
    {
        private readonly Regex _regex = new(@"\d{4}-\d{4}");

        public CustomerValidations()
        {
            RuleFor(x => x.Name)
               .NotEmpty();

            RuleFor(i => i.Code)
                .NotEmpty()
                .Matches(_regex).WithMessage(Resources.FormatErrorCustomerCode);
        }
    }
}
