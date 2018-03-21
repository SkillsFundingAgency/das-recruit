using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;
using FluentValidation;
using FluentValidation.Internal;

namespace FluentValidation
{
    public static class FluentExtensions
    {
        public static IRuleBuilderOptions<T, TElement> RunCondition<T, TElement>(this IConfigurable<PropertyRule, IRuleBuilderOptions<T, TElement>> ruleBuilder, VacancyValidations condition)
        {
            return ruleBuilder.Configure(c => c.ApplyCondition(context => context.CanRunValidator(condition), ApplyConditionTo.AllValidators));
        }

        public static bool CanRunValidator(this ValidationContext context, VacancyValidations validationToCheck)
        {
            var validationsToRun = (VacancyValidations)context.RootContextData["validationsToRun"];

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