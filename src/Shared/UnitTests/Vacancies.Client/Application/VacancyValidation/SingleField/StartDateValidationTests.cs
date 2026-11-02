using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class StartDateValidationTests : VacancyValidationTestsBase
{
    public static IEnumerable<object[]> InvalidDaysFromClosingDate =>
        new List<object[]>
        {
            new object[] { -1 },
            new object[] { -2 },
            new object[] { -15 }
        };

    [Fact]
    public void NoErrorsWhenStartDateIsValid()
    {
        var vacancy = new Vacancy
        {
            ClosingDate = DateTime.Today.AddDays(14), 
            StartDate = DateTime.UtcNow.AddDays(15)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.StartDate);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void StartDateMustHaveAValue()
    {
        var vacancy = new Vacancy 
        {
            StartDate = null
        };
            
        var result = Validator.Validate(vacancy, VacancyRuleSet.StartDate);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.StartDate)}");
        result.Errors[0].ErrorCode.Should().Be("20");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.StartDate);
    }

    [Theory]
    [MemberData(nameof(InvalidDaysFromClosingDate))]
    public void StartDateMustBeGreaterThanToday(int startDate)
    {
        var closingDate = DateTime.Today.AddDays(50);
        var vacancy = new Vacancy
        {
            ClosingDate = closingDate,
            StartDate = closingDate.AddDays(startDate)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.StartDate);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.StartDate)}");
        result.Errors[0].ErrorCode.Should().Be("22");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.StartDate);
    }
}