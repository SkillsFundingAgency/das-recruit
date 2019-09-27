using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;
using FluentValidation;
using FluentValidation.Internal;
using System;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public static class FluentExtensions
    {
        public static IRuleBuilderOptions<T, string> ValidFreeTextCharacters<T>(this IRuleBuilder<T, string> ruleBuilder) {
			return ruleBuilder.SetValidator(new FreeTextValidator());
		}

        public static IRuleBuilderOptions<T, string> ValidHtmlCharacters<T>(this IRuleBuilder<T, string> ruleBuilder, IHtmlSanitizerService sanitizer)
        {
            return ruleBuilder.SetValidator(new HtmlValidator(sanitizer));
        }

        public static IRuleBuilderOptions<T, string> ValidPostCode<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new PostCodeValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidUkprn<T>(this IRuleBuilder<T, string> rule)
        {
            return rule.SetValidator(new UkprnValidator());
        }

        public static IRuleBuilderOptions<T, string> ProfanityCheck<T>(this IRuleBuilder<T, string> rule, IProfanityListProvider profanityListProvider)
        {
            return rule.SetValidator(new ProfanityCheckValidator(profanityListProvider));
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

        internal static IRuleBuilderOptions<T, TElement> WithRuleId<T, TElement>(this IConfigurable<PropertyRule, IRuleBuilderOptions<T, TElement>> ruleBuilder, long ruleId)
        {
            return ruleBuilder.Configure(c =>
            {
                // Set rule type in context so it can be returned in error object
                foreach (var validator in c.Validators)
                {
                    validator.CustomStateProvider = s => ruleId;
                }
            });
        }

        internal static IRuleBuilderInitial<T, TElement> WithRuleId<T, TElement>(this IConfigurable<PropertyRule, IRuleBuilderInitial<T, TElement>> ruleBuilder, long ruleId)
        {
            return ruleBuilder.Configure(c =>
            {
                // Set rule type in context so it can be returned in error object
                foreach (var validator in c.Validators)
                {
                    validator.CustomStateProvider = s => ruleId;
                }
            });
        }
    }
}