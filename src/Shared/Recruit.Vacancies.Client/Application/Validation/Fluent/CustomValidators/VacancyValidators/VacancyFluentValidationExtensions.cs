using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators.VacancyValidators
{
    public static class VacancyFluentValidationExtensions
    {
        public static IRuleBuilderInitial<Vacancy, Vacancy> ClosingDateMustBeLessThanStartDate(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder)
        {
            return ruleBuilder.Custom((vacancy, context) =>
            {
                if (vacancy.StartDate.Value.Date <= vacancy.ClosingDate.Value.Date)
                {
                    var failure = new ValidationFailure(string.Empty, "The possible start date should be after the closing date")
                    {
                        ErrorCode = "24",
                        CustomState = VacancyRuleSet.StartDateEndDate
                    };
                    context.AddFailure(failure);
                }
            });
        }

        public static IRuleBuilderInitial<Vacancy, Vacancy> FixedWageMustBeGreaterThanApprenticeshipMinimumWage(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder, IGetApprenticeshipNationalMinimumWages minimumWageService)
        {
            return ruleBuilder.Custom((vacancy, context) =>
            {

                var apprenticeshipMinWage = minimumWageService.GetMinimumWage(vacancy.StartDate.Value);

                if (vacancy.Wage.FixedWageYearlyAmount == null || vacancy.Wage.FixedWageYearlyAmount / 52 / vacancy.Wage.WeeklyHours < apprenticeshipMinWage)
                {
                    var failure = new ValidationFailure(string.Empty, "The wage should not be less than the new National Minimum Wage for apprentices effective from 1 April 2018")
                    {
                        ErrorCode = "49",
                        CustomState = VacancyRuleSet.MinimumWage
                    };
                    context.AddFailure(failure);
                }
            });
        }
    }
}