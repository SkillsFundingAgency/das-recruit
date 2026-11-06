using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class SkillsTests : VacancyValidationTestsBase
{
    [Test]
    public void NoErrorsWhenSkillsAreValid()
    {
        var vacancy = new Vacancy
        {
            Skills = new List<string>
            {
                new string('a', 30)
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    public static IEnumerable<object[]> NullOrZeroSkillCollection =>
        new List<object[]>
        {
            new object[] {null},
            new object[] {new List<string>()},
        };

    [TestCaseSource(nameof(NullOrZeroSkillCollection))]
    public void SkillsCollectionMustNotBeNullOrHaveZeroCount(List<string> skills)
    {
        var vacancy = new Vacancy
        {
            Skills = skills
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Skills));
        result.Errors[0].ErrorCode.Should().Be("51");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Skills);
    }

    public static IEnumerable<object[]> NullOrZeroSkill =>
        new List<object[]>
        {
            new object[] {new List<string>{null}},
            new object[] {new List<string>{string.Empty}},
        };

    [TestCaseSource(nameof(NullOrZeroSkill))]
    public void SkillMustNotBeEmpty(List<string> skills)
    {
        var vacancy = new Vacancy
        {
            Skills = skills
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Skills)}[0]");
        result.Errors[0].ErrorCode.Should().Be("99");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Skills);
    }

    [Test]
    public void SkillsMustNotContainInvalidCharacters()
    {
        var vacancy = new Vacancy
        {
            Skills = new List<string>
            {
                "<"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Skills)}[0]");
        result.Errors[0].ErrorCode.Should().Be("6");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Skills);
    }

    [Test]
    public void SkillsMustNotBeGreaterThanMaxLength()
    {
        var vacancy = new Vacancy
        {
            Skills = new List<string>
            {
                new string('a', 31)
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Skills)}[0]");
        result.Errors[0].ErrorCode.Should().Be("7");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Skills);
    }

    [TestCase("some text bother")]
    [TestCase("some text dang")]
    [TestCase("some text drat")]
    [TestCase("some text balderdash")]
    public void OutcomeDescription_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            Skills = new List<string>
            {
                freeText
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Skills)}[0]");
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("608");
    }

    [TestCase("some textbother")]
    [TestCase("some textdang")]
    [TestCase("some textdrat")]
    [TestCase("some textbalderdash")]
    public void OutcomeDescription_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            Skills = new List<string>
            {
                freeText
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);
        result.HasErrors.Should().BeFalse();
    }

    [Test]
    public void Skills_Are_Not_Required_For_Foundation_Apprenticeships()
    {
        var vacancy = new Vacancy
        {
            ApprenticeshipType = ApprenticeshipTypes.Foundation,
            Skills = null
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);
        result.HasErrors.Should().BeFalse();
    }
}