using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using SFA.DAS.Recruit.Api.Helpers;
using SFA.DAS.VacancyServices.Wage;
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using WageType = SFA.DAS.Recruit.Api.Models.WageType;

namespace SFA.DAS.Recruit.Api.Extensions
{
    public static class LiveVacancyExtensions
    {
        public static void AddWageData(this LiveVacancy vacancy)
        {
            var wage = new Esfa.Recruit.Vacancies.Client.Domain.Entities.Wage
            {
                FixedWageYearlyAmount = vacancy.Wage.FixedWageYearlyAmount,
                WageType = Enum.Parse<Esfa.Recruit.Vacancies.Client.Domain.Entities.WageType>(vacancy.Wage.WageType),
                WeeklyHours = vacancy.Wage.WeeklyHours
            };

            vacancy.Wage.WageText = wage.ToDisplayText(vacancy.StartDate);

            if (vacancy.Wage.WageType == WageType.FixedWage.ToString())
            {
                vacancy.Wage.ApprenticeMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
                vacancy.Wage.Under18NationalMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
                vacancy.Wage.Between18AndUnder21NationalMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
                vacancy.Wage.Between21AndUnder25NationalMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
                vacancy.Wage.Over25NationalMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
            }
            else if (vacancy.Wage.WageType == WageType.NationalMinimumWageForApprentices.ToString())
            {
                var weeklyHours = (int)decimal.Round(vacancy.Wage.WeeklyHours, MidpointRounding.AwayFromZero);
                var rates = NationalMinimumWageService.GetAnnualRates(vacancy.StartDate, weeklyHours);

                vacancy.Wage.ApprenticeMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Under18NationalMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Between18AndUnder21NationalMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Between21AndUnder25NationalMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Over25NationalMinimumWage = rates.ApprenticeMinimumWage;
            }
            else
            {
                var weeklyHours = (int)decimal.Round(vacancy.Wage.WeeklyHours, MidpointRounding.AwayFromZero);
                var rates = NationalMinimumWageService.GetAnnualRates(vacancy.StartDate, weeklyHours);

                vacancy.Wage.ApprenticeMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Under18NationalMinimumWage = rates.Under18NationalMinimumWage;
                vacancy.Wage.Between18AndUnder21NationalMinimumWage = rates.Between18AndUnder21NationalMinimumWage;
                vacancy.Wage.Between21AndUnder25NationalMinimumWage = rates.Between21AndUnder25NationalMinimumWage;
                vacancy.Wage.Over25NationalMinimumWage = rates.Over25NationalMinimumWage;
            }
        }
    }
}
