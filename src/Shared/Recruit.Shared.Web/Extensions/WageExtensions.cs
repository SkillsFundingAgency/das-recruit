using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.VacancyServices.NationalMinimumWage;
using WageType = Esfa.Recruit.Vacancies.Client.Domain.Entities.WageType;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class WageExtensions
    {
        public static string ToText(this Wage wage, DateTime? expectedStartDate)
        {
            string wageText;

            switch (wage.WageType)
            {
                case WageType.FixedWage:
                    wageText = WagePresenter.GetCustomWageDisplayAmount(wage.FixedWageYearlyAmount).AsMoney();
                    break;
                case WageType.NationalMinimumWage:
                    wageText = WagePresenter.GetNationalMinimumDisplayAmount(WageUnit.Annually, wage.WeeklyHours, expectedStartDate).AsMoney();
                    break;
                case WageType.NationalMinimumWageForApprentices:
                    wageText = WagePresenter.GetApprenticeshipMinimumDisplayAmount(WageUnit.Annually, wage.WeeklyHours, expectedStartDate).AsMoney();
                    break;
                default:
                    wageText = wage.WageType?.GetDisplayName();
                    break;
            }

            return wageText;
        }
    }
}
