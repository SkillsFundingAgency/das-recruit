using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class TrainingDescriptionValidationTests : VacancyValidationTestsBase
{
    [Fact]
    public void NoErrorsWhenTrainingDescriptionFieldIsValid()
    {
        var vacancy = new Vacancy 
        {
            TrainingDescription = "a valid description"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void TrainingDescriptionMustNotBeLongerThanMaxLength()
    {
        var vacancy = new Vacancy 
        {
            TrainingDescription = new String('a', 4001)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingDescription));
        result.Errors[0].ErrorCode.Should().Be("321");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingDescription);
    }

    [Theory]
    [InlineData("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
    [InlineData("<script>alert('not allowed')</script>", false)]
    [InlineData("<p>`</p>", false)]
    public void TrainingDescriptionMustContainValidHtml(string actual, bool expectedResult)
    {
        var vacancy = new Vacancy
        {
            TrainingDescription = actual
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription);

        if (expectedResult)
        {
            result.HasErrors.Should().BeFalse();
        }
        else
        {
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingDescription));
            result.Errors[0].ErrorCode.Should().Be("346");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingDescription);
        }
    }

    [Theory]
    [InlineData("some text bother")]
    [InlineData("some text dang")]
    [InlineData("some text drat")]
    [InlineData("some text balderdash")]
    public void TrainingDescription_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            TrainingDescription = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingDescription));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("322");
    }

    [Theory]
    [InlineData("some textbother")]
    [InlineData("some textdang")]
    [InlineData("some textdrat")]
    [InlineData("some textbalderdash")]
    public void TrainingDescription_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            TrainingDescription = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription);
        result.HasErrors.Should().BeFalse();
    }
}