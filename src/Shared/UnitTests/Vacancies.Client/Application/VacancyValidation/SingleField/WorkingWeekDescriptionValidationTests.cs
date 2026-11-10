using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class WorkingWeekDescriptionValidationTests : VacancyValidationTestsBase
{
    [Test]
    public void NoErrorsWhenWorkingWeekDescriptionValueIsValid()
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WorkingWeekDescription = "This is a valid value"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [TestCase( null)]
    [TestCase("")]
    public void WorkingWeekDescriptionMustHaveAValue(string descriptionValue)
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WorkingWeekDescription = descriptionValue
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
        result.Errors[0].ErrorCode.Should().Be("37");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkingWeekDescription);
    }

    [TestCase( "<")]
    [TestCase( ">")]
    public void WorkingWeekDescriptionMustContainValidCharacters(string invalidCharacter)
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WorkingWeekDescription = new String('a', 50) + invalidCharacter + new String('a', 50)
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
        result.Errors[0].ErrorCode.Should().Be("38");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkingWeekDescription);
    }

    [Test]
    public void WorkingWeekDescriptionMustBeLessThan250Characters()
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WorkingWeekDescription = new string('a', 251)
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
        result.Errors[0].ErrorCode.Should().Be("39");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkingWeekDescription);
    }

    [TestCase("some text bother")]
    [TestCase( "some text dang")]
    [TestCase( "some text drat")]
    [TestCase( "some text balderdash")]
    public void WorkingWeekDescription_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WorkingWeekDescription = freeText
            }
        };
        var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("606");
    }

    [TestCase("some textbother")]
    [TestCase( "some textdang")]
    [TestCase( "some textdrat")]
    [TestCase( "some textbalderdash")]
    public void WorkingWeekDescription_ShouldFail_Not_IfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy
        {
            Wage = new Wage
            {
                WorkingWeekDescription = freeText
            }
        };
        var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);
        result.HasErrors.Should().BeFalse();
    }
}