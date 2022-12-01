using Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class AdditionalQuestion2Tests : VacancyValidationTestsBase
{
    [Fact]
    public void NoErrorsWhenWorkExperienceFieldIsValid()
    {
        var vacancy = new Vacancy 
        {
            AdditionalQuestion2 = "a valid AdditionalQuestion1"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion2);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }
    
    [Fact]
    public void AdditionalQuestion2MustNotBeLongerThanMaxLength()
    {
        var vacancy = new Vacancy 
        {
            AdditionalQuestion2 = new string('a', 251)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion2);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AdditionalQuestion2));
        result.Errors[0].ErrorCode.Should().Be("331");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.AdditionalQuestion2);
    }
    
    [Theory]
    [InlineData("some text bother")]
    [InlineData("some text dang")]
    [InlineData("some text drat")]
    [InlineData("some text balderdash")]
    public void AdditionalQuestion2ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            AdditionalQuestion2 = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion2);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AdditionalQuestion2));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("332");
    }

    [Theory]
    [InlineData("some textbother")]
    [InlineData("some textdang")]
    [InlineData("some textdrat")]
    [InlineData("some textbalderdash")]
    public void AdditionalQuestion2_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            AdditionalQuestion2 = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion2);
        result.HasErrors.Should().BeFalse();
    }
}