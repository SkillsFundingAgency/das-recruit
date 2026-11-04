using Esfa.Recruit.UnitTests.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class EmployerDescriptionValidationTests : VacancyValidationTestsBase
{
    [Test]
    public void NoErrorsWhenEmployerDescriptionFieldIsValid()
    {
        var vacancy = new Vacancy 
        {
            EmployerDescription = "a valid description"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerDescription);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void DescriptionMustNotBeEmpty(string description)
    {
        var vacancy = new Vacancy 
        {
            EmployerDescription = description
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerDescription));
        result.Errors[0].ErrorCode.Should().Be("80");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerDescription);
    }

    [Test]
    public void EmployerDescriptionMustNotBeLongerThanMaxLength()
    {
        var vacancy = new Vacancy 
        {
            EmployerDescription = new String('a', 4001)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerDescription));
        result.Errors[0].ErrorCode.Should().Be("77");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerDescription);
    }

    [Test]
    [TestCase("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
    [TestCase("<script>alert('not allowed')</script>", false)]
    [TestCase("<p>`</p>", false)]
    public void EmployerDescriptionMustContainValidCharacters(string invalidChar, bool expectedResult)
    {
        var vacancy = new Vacancy 
        {
            EmployerDescription = invalidChar
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerDescription);

        if (expectedResult)
        {
            result.HasErrors.Should().BeFalse();
        }
        else
        {
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerDescription));
            result.Errors[0].ErrorCode.Should().Be("78");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerDescription);
        }
    }

    [Test]
    [TestCase("some text bother")]
    [TestCase("some text dang")]
    [TestCase("some text drat")]
    [TestCase("some text balderdash")]
    public void EmployerDescription_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
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

    [Test]
    [TestCase("some textbother")]
    [TestCase("some textdang")]
    [TestCase("some textdrat")]
    [TestCase("some textbalderdash")]
    public void EmployerDescription_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            Description = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Description);
        result.HasErrors.Should().BeFalse();
    }
}
