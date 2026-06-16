using FluentValidation;
using ProSpace.Application.Properties;
using System.Text.RegularExpressions;
using ProSpace.Api.Contracts.Request;

namespace ProSpace.Application.Validations
{
    public class UserValidatior : AbstractValidator<CustomerRegisterRequest>
    {
        private readonly Regex _regex = new(@"\d{4}-\d{4}");

        public UserValidatior()
        {
            RuleFor(x => x.UserName)
               .NotEmpty();

            RuleFor(i => i.UserCode)
                .NotEmpty()
                .Matches(_regex).WithMessage(Resources.CodeFormatError);
        }
    }
}
