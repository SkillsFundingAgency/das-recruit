using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;
using FluentValidation;
using FluentValidation.Internal;
using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    internal static class FluentExtensions
    {
        internal static IRuleBuilderOptions<T, string> ValidFreeTextCharacters<T>(this IRuleBuilder<T, string> ruleBuilder) {
			return ruleBuilder.SetValidator(new FreeTextValidator());
		}

        internal static IRuleBuilderOptions<T, string> PostCode<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new PostCodeValidator());
        }

        internal static bool BeValidWebUrl(string arg)
        {
            Uri result;
            return Uri.TryCreate(arg, UriKind.Absolute, out result)
                   && (result.Scheme.Equals(Uri.UriSchemeHttp) || result.Scheme.Equals(Uri.UriSchemeHttps));
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