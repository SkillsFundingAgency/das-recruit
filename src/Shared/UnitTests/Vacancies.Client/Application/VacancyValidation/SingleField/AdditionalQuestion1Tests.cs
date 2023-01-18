using Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class AdditionalQuestion1Tests : VacancyValidationTestsBase
{
    [Fact]
    public void NoErrorsWhenWorkExperienceFieldIsValid()
    {
        var vacancy = new Vacancy 
        {
            AdditionalQuestion1 = "a valid AdditionalQuestion1?"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }
    
    [Fact]
    public void AdditionalQuestion1MustNotBeLongerThanMaxLength()
    {
        var vacancy = new Vacancy 
        {
            AdditionalQuestion1 = new string('?', 251 )
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AdditionalQuestion1));
        result.Errors[0].ErrorCode.Should().Be("321");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.AdditionalQuestion1);
    }
    
    [Theory]
    [InlineData("some text bother?")]
    [InlineData("some text dang?")]
    [InlineData("some text drat?")]
    [InlineData("some text balderdash?")]
    public void AdditionalQuestion1_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            AdditionalQuestion1 = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AdditionalQuestion1));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("322");
    }

    [Theory]
    [InlineData("some textbother?")]
    [InlineData("some textdang?")]
    [InlineData("some textdrat?")]
    [InlineData("some textbalderdash?")]
    public void AdditionalQuestion1_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            AdditionalQuestion1 = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1);
        result.HasErrors.Should().BeFalse();
    }

    [Theory]
    [InlineData("some text?")]
    [InlineData("some text?")]
    [InlineData("some text?")]
    [InlineData("some text?")]
    public void AdditionalQuestion1_MustContainQuestionMark(string freeText)
    {
        var vacancy = new Vacancy()
        {
            AdditionalQuestion1 = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1);
        result.HasErrors.Should().BeFalse();
    }

    [Theory]
    [InlineData("some text")]
    [InlineData("some text")]
    [InlineData("some text")]
    [InlineData("some text")]
    public void AdditionalQuestion1_ShouldHaveErrorsIfDoesNotHaveQuestionMark(string freeText)
    {
        var vacancy = new Vacancy()
        {
            AdditionalQuestion1 = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AdditionalQuestion1));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("340");
    }
}