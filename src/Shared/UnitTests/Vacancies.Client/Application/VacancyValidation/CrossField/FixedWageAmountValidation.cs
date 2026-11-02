using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField;

public class FixedWageAmountValidation : VacancyValidationTestsBase
{
    [Theory]
    [InlineData(3.70, 40, 12000.00)] // £5.77 per hour
    public void FixedWageAmountIsValidIfOverMinimumWage(float minimumWageTestValue, int hoursPerWeekValue, float yearlyWageAmcountValue)
    {
        var startDate = DateTime.UtcNow.Date;
        var minimumWageAmount = Convert.ToDecimal(minimumWageTestValue);

        var vacancy = new Vacancy
        {
            StartDate = startDate,
            Wage = new Wage
            {
                WageType = WageType.FixedWage,
                FixedWageYearlyAmount = Convert.ToDecimal(yearlyWageAmcountValue),
                WeeklyHours = hoursPerWeekValue
            }
        };

        MockMinimumWageService.Setup(x => 
            x.GetWagePeriod(It.IsAny<DateTime>())).Returns(new MinimumWage{ApprenticeshipMinimumWage = minimumWageAmount});

        var result = Validator.Validate(vacancy, VacancyRuleSet.MinimumWage);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Theory]
    [InlineData(3.70, 40, "7000.00")] // £3.37 per hour
    [InlineData(3.70, 40, null)]
    public void FixedWageAmountIsNotValidIfUnderMinimumWage(float minimumWageTestValue, int hoursPerWeekValue, string yearlyWageAmcountValue)
    {
        var startDate = DateTime.UtcNow.Date;
        var minimumWageAmount = Convert.ToDecimal(minimumWageTestValue);

        var vacancy = new Vacancy
        {
            StartDate = startDate,
            Wage = new Wage
            {
                WageType = WageType.FixedWage,
                FixedWageYearlyAmount = yearlyWageAmcountValue != null ? Convert.ToDecimal(yearlyWageAmcountValue) : default(decimal?),
                WeeklyHours = hoursPerWeekValue
            }
        };

        MockMinimumWageService.Setup(x =>
            x.GetWagePeriod(It.IsAny<DateTime>())).Returns(new MinimumWage { ApprenticeshipMinimumWage = minimumWageAmount });


        var result = Validator.Validate(vacancy, VacancyRuleSet.MinimumWage);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(Wage)}.{nameof(Wage.FixedWageYearlyAmount)}" );
        result.Errors[0].ErrorCode.Should().Be("49");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.MinimumWage);
    }
}