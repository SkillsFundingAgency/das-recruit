using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class OutcomeDescriptionValidationTests : VacancyValidationTestsBase
{
    [Fact]
    public void NoErrorsWhenOutcomeDescriptionFieldIsValid()
    {
        var vacancy = new Vacancy 
        {
            OutcomeDescription = "a valid description"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.OutcomeDescription);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void OutcomeDescriptionMustNotBeEmpty(string outcomeDescription)
    {
        var vacancy = new Vacancy 
        {
            OutcomeDescription = outcomeDescription
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.OutcomeDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.OutcomeDescription));
        result.Errors[0].ErrorCode.Should().Be("55");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OutcomeDescription);
    }

    [Fact]
    public void OutcomeDescriptionMustNotBeLongerThanMaxLength()
    {
        var vacancy = new Vacancy 
        {
            OutcomeDescription = new String('a', 4001)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.OutcomeDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.OutcomeDescription));
        result.Errors[0].ErrorCode.Should().Be("7");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OutcomeDescription);
    }

    [Theory]
    [InlineData("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
    [InlineData("<script>alert('not allowed')</script>", false)]
    [InlineData("<p>`</p>", false)]
    public void OutcomeDescriptionMustContainValidHtml(string actual, bool expectedResult)
    {
        var vacancy = new Vacancy
        {
            OutcomeDescription = actual
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.OutcomeDescription);

        if (expectedResult)
        {
            result.HasErrors.Should().BeFalse();
        }
        else
        {
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.OutcomeDescription));
            result.Errors[0].ErrorCode.Should().Be("6");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OutcomeDescription);
        }
    }

    [Theory]
    [InlineData("some text bother")]
    [InlineData("some text dang")]
    [InlineData("some text drat")]
    [InlineData("some text balderdash")]
    public void OutcomeDescription_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            OutcomeDescription = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.OutcomeDescription);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.OutcomeDescription));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("611");
    }

    [Theory]
    [InlineData("some textbother")]
    [InlineData("some textdang")]
    [InlineData("some textdrat")]
    [InlineData("some textbalderdash")]
    public void OutcomeDescription_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            OutcomeDescription = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.OutcomeDescription);
        result.HasErrors.Should().BeFalse();
    }
}