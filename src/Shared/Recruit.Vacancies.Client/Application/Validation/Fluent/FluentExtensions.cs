using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;
using FluentValidation.Internal;

namespace FluentValidation
{
    public static class FluentExtensions
    {
        public static IRuleBuilderOptions<T, TElement> RunCondition<T, TElement>(this IConfigurable<PropertyRule, IRuleBuilderOptions<T, TElement>> ruleBuilder, VacancyRuleSet condition)
        {
            return ruleBuilder.Configure(c => c.ApplyCondition(context => context.CanRunValidator(condition), ApplyConditionTo.AllValidators));
        }

        public static IRuleBuilderInitial<T, TElement> RunCondition<T, TElement>(this IConfigurable<PropertyRule, IRuleBuilderInitial<T, TElement>> ruleBuilder, VacancyRuleSet condition)
        {
            return ruleBuilder.Configure(c => c.ApplyCondition(context => context.CanRunValidator(condition), ApplyConditionTo.AllValidators));
        }

        public static IRuleBuilderOptions<T, TElement> WithRuleId<T, TElement>(this IConfigurable<PropertyRule, IRuleBuilderOptions<T, TElement>> ruleBuilder, long ruleId)
        {
            return ruleBuilder.Configure(c =>
            {
                // Set rule type in context so can be returned in error object
                foreach (var validator in c.Validators)
                {
                    validator.CustomStateProvider = s => ruleId;
                }
            });
        }

        public static IRuleBuilderInitial<T, TElement> WithRuleId<T, TElement>(this IConfigurable<PropertyRule, IRuleBuilderInitial<T, TElement>> ruleBuilder, long ruleId)
        {
            return ruleBuilder.Configure(c =>
            {
                // Set rule type in context so can be returned in error object
                foreach (var validator in c.Validators)
                {
                    validator.CustomStateProvider = s => ruleId;
                }
            });
        }

        public static bool CanRunValidator(this ValidationContext context, VacancyRuleSet validationToCheck)
        {
            var validationsToRun = (VacancyRuleSet)context.RootContextData[ValidationConstants.ValidationsRulesKey];

            return (validationsToRun & validationToCheck) > 0;
        }

        public static IRuleBuilderOptions<T, string> ValidFreeTextCharacters<T>(this IRuleBuilder<T, string> ruleBuilder) {
			return ruleBuilder.SetValidator(new FreeTextValidator());
		}

        public static IRuleBuilderOptions<T, string> PostCode<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new PostCodeValidator());
        }
    }
}