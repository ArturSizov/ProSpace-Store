using FluentValidation;
using ProSpace.Application.Properties;
using ProSpace.Contracts.DTO;
using System.Text.RegularExpressions;

namespace ProSpace.Application.Validations
{
    public class ItemValidatior : AbstractValidator<ItemDto>
    {
        private readonly Regex _regex= new(@"\d{2}-\d{4}-[A-Z]{2}\d{2}");

        public ItemValidatior()
        {
            RuleFor(i => i.Name)
               .NotEmpty();

            RuleFor(i => i.Code)
                .NotEmpty()
                .WithName(Resources.ProductaCode)
                .Matches(_regex)
                .WithFormat(Resources.CodeFormatError, "11-1111-SD11");


            RuleFor(x => x.Category)
               .NotEmpty()
               .WithName(Resources.Category)
               .MinimumLength(2)
               .MaximumLength(30);
        }
    }
}
