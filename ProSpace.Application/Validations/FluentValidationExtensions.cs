using FluentValidation;

namespace ProSpace.Application.Validations
{
    /// <summary>
    /// Provides static extension methods for FluentValidation to simplify message formatting and localization.
    /// </summary>
    public static class FluentValidationExtensions
    {
        /// <summary>
        /// Binds a localized message template from resources and dynamically injects a custom <c>{ExpectedFormat}</c> 
        /// placeholder without breaking standard FluentValidation placeholders like <c>{PropertyName}</c>.
        /// </summary>
        /// <typeparam name="T">The type of the model or DTO being validated.</typeparam>
        /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
        /// <param name="rule">The current FluentValidation rule builder options instance.</param>
        /// <param name="resourceMessage">The raw message template fetched from the resource file (e.g., <c>Resources.FormatErrorCommon</c>).</param>
        /// <param name="formatHint">The plain text hint indicating the correct format (e.g., <c>"12-3456-AB78"</c>) to replace the <c>{ExpectedFormat}</c> placeholder.</param>
        /// <returns>The rule builder options instance for method chaining.</returns>
        /// <example>
        /// Usage example:
        /// <code>
        /// RuleFor(x => x.UserCode)
        ///     .Matches(_regex)
        ///     .WithFormat(Resources.FormatErrorCommon, "12-3456-AB78");
        /// </code>
        /// Template in the .resx file:
        /// <c>'{PropertyName}' is not a valid format. Expected: '{ExpectedFormat}'</c>
        /// </example>
        public static IRuleBuilderOptions<T, TProperty> WithFormat<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            string resourceMessage,
            string formatHint)
        {
            return rule.Configure(config =>
            {
                config.MessageBuilder = context =>
                {
                    context.MessageFormatter.AppendArgument("ExpectedFormat", formatHint);

                    return context.MessageFormatter.BuildMessage(resourceMessage);
                };
            });
        }
    }
}
