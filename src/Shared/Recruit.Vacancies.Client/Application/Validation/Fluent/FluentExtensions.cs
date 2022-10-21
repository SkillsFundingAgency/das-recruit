using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;
using FluentValidation;
using System;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public static class FluentExtensions
    {
        public static IRuleBuilderOptions<T, string> ValidFreeTextCharacters<T>(this IRuleBuilder<T, string> ruleBuilder) {
			return ruleBuilder.SetValidator(new FreeTextValidator<T, string>());
		}

        public static IRuleBuilderOptions<T, string> ValidHtmlCharacters<T>(this IRuleBuilder<T, string> ruleBuilder, IHtmlSanitizerService sanitizer)
        {
            return ruleBuilder.SetValidator(new HtmlValidator<T, string>(sanitizer));
        }

        public static IRuleBuilderOptions<T, string> ValidPostCode<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new PostCodeValidator<T, string>());
        }

        public static IRuleBuilderOptions<T, string> ValidUkprn<T>(this IRuleBuilder<T, string> rule)
        {
            return rule.SetValidator(new UkprnValidator<T, string>());
        }

        public static IRuleBuilderOptions<T, string> ProfanityCheck<T>(this IRuleBuilder<T, string> rule, IProfanityListProvider profanityListProvider)
        {
            return rule.SetValidator(new ProfanityCheckValidator<T, string>(profanityListProvider));
        }

        internal static bool BeValidWebUrl(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return false;

            // cannot contain spaces
            if (arg.Contains(" "))
                return false;

            if (arg.StartsWith(".") || arg.EndsWith("."))
                return false;

            // must have a period
            if (!arg.Contains("."))
                return false;

            return Uri.TryCreate(arg, UriKind.RelativeOrAbsolute, out _);
        }
    }
}