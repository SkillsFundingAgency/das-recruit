using Esfa.Recruit.Vacancies.Client.Application.Validation;
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
    }
}