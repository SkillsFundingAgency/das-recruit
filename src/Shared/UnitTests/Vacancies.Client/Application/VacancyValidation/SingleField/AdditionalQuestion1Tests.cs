using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class AdditionalQuestion1Tests : VacancyValidationTestsBase
{
    [Theory]
    [InlineData("a valid AdditionalQuestion1?")]
    [InlineData("a valid? AdditionalQuestion1")]
    [InlineData("")]
    public void NoErrorsWhenAdditionalQuestion1FieldIsValid(string text)
    {
        var vacancy = new Vacancy 
        {
            AdditionalQuestion1 = text
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion2);
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

    [Fact]
    public void AdditionalQuestion1_MustContainQuestionMark()
    {
        var vacancy = new Vacancy
        {
            AdditionalQuestion1 = "some text?"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1);
        result.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void AdditionalQuestion1_ShouldHaveErrorsIfDoesNotHaveQuestionMark()
    {
        var vacancy = new Vacancy
        {
            AdditionalQuestion1 = "some text"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AdditionalQuestion1));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("340");
    }
}