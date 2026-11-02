using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField;

public class StartDateClosingDateTests : VacancyValidationTestsBase
{
    [Fact]
    public void NoErrorsWhenFieldsAreValid()
    {
        var vacancy = new Vacancy
        {
            StartDate = DateTime.UtcNow.AddDays(5),
            ClosingDate = DateTime.UtcNow.AddDays(4)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.StartDateEndDate);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void StartDateCantBeTheSameAsClosingDate()
    {
        var vacancy = new Vacancy
        {
            StartDate = DateTime.UtcNow,
            ClosingDate = DateTime.UtcNow
        };
            
        var result = Validator.Validate(vacancy, VacancyRuleSet.StartDateEndDate);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(string.Empty);
        result.Errors[0].ErrorCode.Should().Be("24");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.StartDateEndDate);
    }

    [Fact]
    public void StartDateCantBeBeforeClosingDate()
    {
        var vacancy = new Vacancy
        {
            StartDate = DateTime.UtcNow,
            ClosingDate = DateTime.UtcNow.AddDays(1)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.StartDateEndDate);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(string.Empty);
        result.Errors[0].ErrorCode.Should().Be("24");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.StartDateEndDate);
    }
}