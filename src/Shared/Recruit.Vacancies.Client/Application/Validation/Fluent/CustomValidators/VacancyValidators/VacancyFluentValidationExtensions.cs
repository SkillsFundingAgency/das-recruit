using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
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

        public static IRuleBuilderInitial<Vacancy, Vacancy> TrainingMustBeActiveForStartDate(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder, Lazy<IEnumerable<ApprenticeshipProgramme>> programmes)
        {
            return ruleBuilder.Custom((vacancy, context) =>
            {
                var matchingProgramme = programmes.Value.SingleOrDefault(x => x.Id.Equals(vacancy.Programme.Id, StringComparison.InvariantCultureIgnoreCase));

                if (matchingProgramme.EffectiveTo != null && matchingProgramme.EffectiveTo < vacancy.StartDate)
                {
                    var failure = new ValidationFailure(string.Empty, "This [framework/standard] is no longer available on the date selected. Choose other apprenticeship training or change the start date.")
                    {
                        ErrorCode = "26",
                        CustomState = VacancyRuleSet.TrainingExpiryDate
                    };
                    context.AddFailure(failure);
                }
            });
        }
    }
}