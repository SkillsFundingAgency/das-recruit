using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class PositionValidationTests : VacancyValidationTestsBase
{
    [Test]
    public void NoErrorsWhenPositionFieldsAreValid()
    {
        var vacancy = new Vacancy
        {
            NumberOfPositions = 2,
            ShortDescription = new string('a', 60)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.NumberOfPositions | VacancyRuleSet.ShortDescription);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Test]
    [TestCase(null)]
    [TestCase(0)]
    public void NumberOfPositionMustHaveAValue(int? numOfPositionsValue)
    {
        var vacancy = new Vacancy 
        {
            NumberOfPositions = numOfPositionsValue
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.NumberOfPositions);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.NumberOfPositions));
        result.Errors[0].ErrorCode.Should().Be("10");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.NumberOfPositions);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void ShortDescriptionMustHaveAValue(string shortDescriptionValue)
    {
        var vacancy = new Vacancy
        {
            ShortDescription = shortDescriptionValue
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
        result.Errors[0].ErrorCode.Should().Be("12");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
    }

    [Test]
    public void NoErrorsWhenShortDescriptionAreValid()
    {
        var vacancy = new Vacancy
        {
            ShortDescription = new String('a', 350)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Test]
    public void ShortDescriptionMustNotBeMoreThan350Characters()
    {
        var vacancy = new Vacancy
        {
            ShortDescription = new String('a', 351)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
        result.Errors[0].ErrorCode.Should().Be("13");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
    }

    [Test]
    public void ShortDescriptionMustNotBeLessThan50Characters()
    {
        var vacancy = new Vacancy
        {
            ShortDescription = new String('a', 49)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
        result.Errors[0].ErrorCode.Should().Be("14");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
    }


    [Test]
    [TestCase("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
    [TestCase("<script>alert('not allowed')</script>", false)]
    [TestCase("<p>`</p>", false)]
    public void ShortDescriptionMustContainValidCharacters(string invalidCharacter, bool expectedResult)
    {
        var vacancy = new Vacancy
        {
            ShortDescription = new String('a', 30) + invalidCharacter + new String('b', 30)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

        if (expectedResult)
        {
            result.HasErrors.Should().BeFalse();
        }
        else
        {
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors[0].ErrorCode.Should().Be("15");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }
    }

    [Test]
    [TestCase("some text bother random text random text random text random text")]
    [TestCase("some text dang random text random text random text random text")]
    [TestCase("some text drat random text random text random text random text")]
    [TestCase("some text balderdash random text random text random text random text")]
    public void ShortDescription_Should_Fail_IfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            ShortDescription = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("605");
    }

    [Test]
    [TestCase("some textbother random text random text random text random text")]
    [TestCase("some textdang random text random text random text random text")]
    [TestCase("some textdrat random text random text random text random text")]
    [TestCase("some textbalderdash random text random text random text random text")]
    public void ShortDescription_Should_Not_Fail_IfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            ShortDescription = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);
        result.HasErrors.Should().BeFalse();
    }
}
