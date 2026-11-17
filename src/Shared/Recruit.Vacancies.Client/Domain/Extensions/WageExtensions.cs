using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.VacancyServices.Wage;
using WageType = Esfa.Recruit.Vacancies.Client.Domain.Entities.WageType;

namespace Esfa.Recruit.Vacancies.Client.Domain.Extensions;

public static class WageExtensions
{
    public static string ToDisplayText(this Wage wage, DateTime? expectedStartDate)
    {
        var wageDetails = new WageDetails
        {
            Amount = wage.FixedWageYearlyAmount,
            HoursPerWeek = wage.WeeklyHours,
            StartDate = expectedStartDate.GetValueOrDefault()
        };
        string wageText;

        switch (wage.WageType)
        {
            case WageType.FixedWage:
                wageText = WagePresenter
                    .GetDisplayText(SFA.DAS.VacancyServices.Wage.WageType.Custom, WageUnit.Annually, wageDetails)
                    .AsWholeMoneyAmount()
                    .AsPerYear();
                break;
            case WageType.NationalMinimumWage:
                wageText = WagePresenter
                    .GetDisplayText(SFA.DAS.VacancyServices.Wage.WageType.NationalMinimum, WageUnit.Annually, wageDetails)
                    .AsWholeMoneyAmount()
                    .AsPerYear();
                break;
            case WageType.NationalMinimumWageForApprentices:
                wageText = WagePresenter
                    .GetDisplayText(SFA.DAS.VacancyServices.Wage.WageType.ApprenticeshipMinimum, WageUnit.Annually, wageDetails)
                    .AsWholeMoneyAmount()
                    .AsPerYear();
                break;
            case WageType.CompetitiveSalary:
                wageText = "Competitive";
                break;
            default:
                wageText = wage.WageType.ToString();
                break;
        }

        return wageText;
    }
}