using FluentValidation;
using ProSpace.Api.Contracts.Request;
using ProSpace.Application.Properties;

namespace ProSpace.Application.Validations
{
    public class UserValidatior : AbstractValidator<CustomerRegisterRequest>
    {
        public UserValidatior()
        {
            RuleFor(x => x.Email)
              .EmailAddress();

            RuleFor(x => x.UserName)
               .NotEmpty()
               .WithName(Resources.Name);
        }
    }
}
