using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class TrainingDescriptionValidationTests : VacancyValidationTestsBase
{
    [Test]
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

    [Test]
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

    [Test]
    [TestCase("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
    [TestCase("<script>alert('not allowed')</script>", false)]
    [TestCase("<p>`</p>", false)]
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

    [Test]
    [TestCase("some text bother")]
    [TestCase("some text dang")]
    [TestCase("some text drat")]
    [TestCase("some text balderdash")]
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

    [Test]
    [TestCase("some textbother")]
    [TestCase("some textdang")]
    [TestCase("some textdrat")]
    [TestCase("some textbalderdash")]
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
