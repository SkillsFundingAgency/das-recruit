using System;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using SFA.DAS.Recruit.Api.Extensions;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.UnitTests.Extensions;

public class LiveVacancyExtensionTests
{
    [Test, AutoData]
    public void Then_The_Wage_Is_Correctly_Calculated_For_Apprentice_Minimum_Wage(LiveVacancy vacancy)
    {
        vacancy.Wage.WeeklyHours = 37.5m;
        vacancy.Wage.WageType = nameof(WageType.NationalMinimumWageForApprentices);
        vacancy.StartDate = new DateTime(2025, 08, 26);
        
        vacancy.AddWageData();

        vacancy.Wage.WeeklyHours.Should().Be(37.5m);
        vacancy.Wage.WageText.Should().Be("£14,722.50 a year");
        vacancy.Wage.ApprenticeMinimumWage.Should().Be(14722.50m);
    }
    
    [Test, AutoData]
    public void Then_The_Wage_Is_Correctly_Calculated_For_Minimum_Wage(LiveVacancy vacancy)
    {
        vacancy.Wage.WeeklyHours = 37.5m;
        vacancy.Wage.WageType = nameof(WageType.NationalMinimumWage);
        vacancy.StartDate = new DateTime(2025, 08, 26);
        
        vacancy.AddWageData();

        vacancy.Wage.WeeklyHours.Should().Be(37.5m);
        vacancy.Wage.WageText.Should().Be("£14,722.50 to £23,809.50 a year");
        vacancy.Wage.ApprenticeMinimumWage.Should().Be(14722.50m);
        vacancy.Wage.Under18NationalMinimumWage.Should().Be(14722.50m);
        vacancy.Wage.Between18AndUnder21NationalMinimumWage.Should().Be(19500m);
        vacancy.Wage.Between21AndUnder25NationalMinimumWage.Should().Be(23809.50m);
        vacancy.Wage.Over25NationalMinimumWage.Should().Be(23809.50m);
    }
}