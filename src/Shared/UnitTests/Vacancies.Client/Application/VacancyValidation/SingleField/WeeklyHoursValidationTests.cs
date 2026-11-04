using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class WeeklyHoursValidationTests : VacancyValidationTestsBase
{
    [Test]
    public void NoErrorsWhenWeeklyHoursIsValid()
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WeeklyHours = 30
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.WeeklyHours);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Test]
    public void WeeklyHoursMustHaveAValue()
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WeeklyHours = null
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.WeeklyHours);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WeeklyHours)}");
        result.Errors[0].ErrorCode.Should().Be("40");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WeeklyHours);
    }

    [Test]
    public void WeeklyHoursMustBeMoreThan16()
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WeeklyHours = 15
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.WeeklyHours);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WeeklyHours)}");
        result.Errors[0].ErrorCode.Should().Be("42");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WeeklyHours);
    }

    [Test]
    public void WeeklyHoursMustBeLeeThan48()
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WeeklyHours = 49
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.WeeklyHours);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WeeklyHours)}");
        result.Errors[0].ErrorCode.Should().Be("43");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WeeklyHours);
    }
}
