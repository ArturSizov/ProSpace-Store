using FluentValidation;
using ProSpace.Application.Properties;
using ProSpace.Domain.Models;
using System.Text.RegularExpressions;

namespace ProSpace.Application.Validations
{
    public class ItemValidations : AbstractValidator<ItemModel>
    {
        private readonly Regex _regex= new(@"\d{2}-\d{4}-[A-Z]{2}\d{2}");

        public ItemValidations()
        {
            RuleFor(i => i.Name)
               .NotEmpty();

            RuleFor(i => i.Code)
                .NotEmpty()
                .Matches(_regex).WithMessage($"'Code' {Resources.FormatErrorItemCode} ");


            RuleFor(x => x.Category)
               .NotEmpty()
               .MaximumLength(30);
        }
    }
}
