using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField;

public class FixedWageAmountValidation : VacancyValidationTestsBase
{
    [Test]
    [TestCase(3.70, 40, 12000.00)] // £5.77 per hour
    public void FixedWageAmountIsValidIfOverMinimumWage(double minimumWageTestValue, int hoursPerWeekValue, double yearlyWageAmountValue)
    {
        var startDate = DateTime.UtcNow.Date;
        var minimumWageAmount = Convert.ToDecimal(minimumWageTestValue);

        var vacancy = new Vacancy
        {
            StartDate = startDate,
            Wage = new Wage
            {
                WageType = WageType.FixedWage,
                FixedWageYearlyAmount = Convert.ToDecimal(yearlyWageAmountValue),
                WeeklyHours = hoursPerWeekValue
            }
        };

        MockMinimumWageService.Setup(x => 
            x.GetWagePeriod(It.IsAny<DateTime>())).Returns(new MinimumWage{ApprenticeshipMinimumWage = minimumWageAmount});

        var result = Validator.Validate(vacancy, VacancyRuleSet.MinimumWage);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Test]
    [TestCase(3.70, 40, "7000.00")] // £3.37 per hour
    [TestCase(3.70, 40, null)]
    public void FixedWageAmountIsNotValidIfUnderMinimumWage(double minimumWageTestValue, int hoursPerWeekValue, string yearlyWageAmountValue)
    {
        var startDate = DateTime.UtcNow.Date;
        var minimumWageAmount = Convert.ToDecimal(minimumWageTestValue);

        var vacancy = new Vacancy
        {
            StartDate = startDate,
            Wage = new Wage
            {
                WageType = WageType.FixedWage,
                FixedWageYearlyAmount = yearlyWageAmountValue != null ? Convert.ToDecimal(yearlyWageAmountValue) : default(decimal?),
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
