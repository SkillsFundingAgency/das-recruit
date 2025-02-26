using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class HowWillTheApprenticeTrainTests : VacancyValidationTestsBase
{
    [Test]
    public void NoErrorsWhenDescriptionFieldIsValid()
    {
        var vacancy = new Vacancy 
        {
            TrainingDescription = "a valid description",
            AdditionalTrainingDescription = "a valid description"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription | VacancyRuleSet.AdditionalTrainingDescription);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }        
    
    [Test]
    public void DescriptionMustNotBeLongerThanMaxLength()
    {
        var vacancy = new Vacancy 
        {
            TrainingDescription = new String('a', 4001),
            AdditionalTrainingDescription = new String('a', 4001),
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription | VacancyRuleSet.AdditionalTrainingDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingDescription));
        result.Errors[0].ErrorCode.Should().Be("321");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingDescription);
        result.Errors[1].PropertyName.Should().Be(nameof(vacancy.AdditionalTrainingDescription));
        result.Errors[1].ErrorCode.Should().Be("341");
        result.Errors[1].RuleId.Should().Be((long)VacancyRuleSet.AdditionalTrainingDescription);
    }

    [TestCase("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
    [TestCase("<script>alert('not allowed')</script>", false)]
    [TestCase("<p>`</p>", false)]
    public void DescriptionMustContainValidHtml(string actual, bool expectedResult)
    {
        var vacancy = new Vacancy 
        {
            TrainingDescription = actual,
            AdditionalTrainingDescription = actual
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription | VacancyRuleSet.AdditionalTrainingDescription);

        if (expectedResult)
        {
            result.HasErrors.Should().BeFalse();
        }
        else
        {
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(2);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingDescription));
            result.Errors[0].ErrorCode.Should().Be("346");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingDescription);
            result.Errors[1].PropertyName.Should().Be(nameof(vacancy.AdditionalTrainingDescription));
            result.Errors[1].ErrorCode.Should().Be("344");
            result.Errors[1].RuleId.Should().Be((long)VacancyRuleSet.AdditionalTrainingDescription);
        }
    }

    [TestCase("some text bother")]
    [TestCase("some text dang")]
    [TestCase("some text drat")]
    [TestCase("some text balderdash")]
    public void Description_Should_Fail_IfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            TrainingDescription = freeText,
            AdditionalTrainingDescription = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription | VacancyRuleSet.AdditionalTrainingDescription);
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingDescription));
        result.Errors[0].ErrorCode.Should().Be("322");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingDescription);
        result.Errors[1].PropertyName.Should().Be(nameof(vacancy.AdditionalTrainingDescription));
        result.Errors[1].ErrorCode.Should().Be("342");
        result.Errors[1].RuleId.Should().Be((long)VacancyRuleSet.AdditionalTrainingDescription);
    }

    [TestCase("some textbother")]
    [TestCase("some textdang")]
    [TestCase("some textdrat")]
    [TestCase("some textbalderdash")]
    public void Description_Should_Not_Fail_IfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            TrainingDescription = freeText,
            AdditionalTrainingDescription = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription | VacancyRuleSet.AdditionalTrainingDescription);
        result.HasErrors.Should().BeFalse();
    }
}