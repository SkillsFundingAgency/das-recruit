using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class OutcomeDescriptionValidationTests : VacancyValidationTestsBase
{
    [Test]
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

    [Test]
    [TestCase(null)]
    [TestCase("")]
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

    [Test]
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

    [Test]
    [TestCase("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
    [TestCase("<script>alert('not allowed')</script>", false)]
    [TestCase("<p>`</p>", false)]
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

    [Test]
    [TestCase("some text bother")]
    [TestCase("some text dang")]
    [TestCase("some text drat")]
    [TestCase("some text balderdash")]
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

    [Test]
    [TestCase("some textbother")]
    [TestCase("some textdang")]
    [TestCase("some textdrat")]
    [TestCase("some textbalderdash")]
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
