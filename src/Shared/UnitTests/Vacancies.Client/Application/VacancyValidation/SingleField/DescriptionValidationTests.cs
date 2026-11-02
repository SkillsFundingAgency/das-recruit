using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class DescriptionValidationTests : VacancyValidationTestsBase
{
    [Fact]
    public void NoErrorsWhenDescriptionFieldIsValid()
    {
        var vacancy = new Vacancy 
        {
            Description = "a valid description"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Description);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }
        
    [Theory]
    [InlineData(null, "will do at work")]
    [InlineData("", "will do at work")]
    public void DescriptionMustNotBeEmpty(string description, string errorMessage)
    {
        var vacancy = new Vacancy 
        {
            Description = description
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Description);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Description));
        result.Errors[0].ErrorCode.Should().Be("53");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Description);
        result.Errors[0].ErrorMessage.Should().Contain(errorMessage);
    }

    [Fact]
    public void DescriptionMustNotBeLongerThanMaxLength()
    {
        var vacancy = new Vacancy 
        {
            Description = new String('a', 4001)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Description);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Description));
        result.Errors[0].ErrorCode.Should().Be("7");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Description);
    }

    [Theory]
    [InlineData("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
    [InlineData("<script>alert('not allowed')</script>", false)]
    [InlineData("<p>`</p>", false)]
    public void DescriptionMustContainValidHtml(string actual, bool expectedResult)
    {
        var vacancy = new Vacancy 
        {
            Description = actual
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Description);

        if (expectedResult)
        {
            result.HasErrors.Should().BeFalse();
        }
        else
        {
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Description));
            result.Errors[0].ErrorCode.Should().Be("6");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Description);
        }
    }

    [Theory]
    [InlineData("some text bother")]
    [InlineData("some text dang")]
    [InlineData("some text drat")]
    [InlineData("some text balderdash")]
    public void Description_Should_Fail_IfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            Description = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Description);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Description));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("609");
    }

    [Theory]
    [InlineData("some textbother")]
    [InlineData("some textdang")]
    [InlineData("some textdrat")]
    [InlineData("some textbalderdash")]
    public void Description_Should_Not_Fail_IfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            Description = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Description);
        result.HasErrors.Should().BeFalse();
    }
}