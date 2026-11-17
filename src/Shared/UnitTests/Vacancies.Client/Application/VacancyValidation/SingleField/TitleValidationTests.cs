using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class ValidateVacancyTests : VacancyValidationTestsBase
{
    public static IEnumerable<object[]> ValidTitles =>
        new List<object[]>
        {
            new object[] { $"apprentice {new string('a', 89)}" },
            new object[] { "apprentice" }
        };

    [TestCaseSource(nameof(ValidTitles))]
    public void NoErrorsWhenTitleFieldIsValidForApprenticeship(string validTitle)
    {
        var vacancy = new Vacancy 
        {
            Title = validTitle
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }
        

    [TestCase(null)]
    [TestCase("")]
    public void TitleMustHaveAValue(string titleValue)
    {
        var vacancy = new Vacancy 
        {
            Title = titleValue
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
        result.Errors[0].ErrorCode.Should().Be("1");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
    }

    [TestCase("Apprentice mage")]
    [TestCase("Apprenticeship in sorcery")]
    [TestCase("Mage apprentice")]
    [TestCase("Witchcraft apprenticeship")]
    [TestCase("junior apprentice mage")]
    [TestCase("junior apprenticeship in sorcery")]
    public void NoErrorsWhenTitleContainsTheWordApprenticeOrApprenticeship(string testValue)
    {
        var vacancy = new Vacancy
        {
            Title = testValue
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

        result.HasErrors.Should().BeFalse();
    }
        
    [TestCase("mage")]
    [TestCase("Apprenticeshipin sorcery")]
    [TestCase("Mage apprenticesip")]
    [TestCase("Witchcraft aprentice")]
    [TestCase("aprentice mage")]
    [TestCase("junior apprenteeship in sorcery")]
    public void TitleMustContainTheWordApprenticeOrApprenticeship(string testValue)
    {
        var vacancy = new Vacancy
        {
            Title = testValue
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
        result.Errors[0].ErrorCode.Should().Be("200");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
    }
        
    [Test]
    public void TitleBeLongerThan100Characters()
    {
        var vacancy = new Vacancy 
        {
            Title = $"apprentice {new string('a', 101)}"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
        result.Errors[0].ErrorCode.Should().Be("2");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
    }

    [TestCase("apprentice<")]
    [TestCase("apprentice>" )]
    public void TitleMustContainValidCharacters(string testValue)
    {
        var vacancy = new Vacancy 
        {
            Title = testValue
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
        result.Errors[0].ErrorCode.Should().Be("3");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
    }

    [TestCase("some text bother apprentice")]
    [TestCase("some text dang apprentice")]
    [TestCase("some text drat apprentice")]
    [TestCase("some text balderdash apprentice")]
    public void Title_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            Title = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Title);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("601");
    }

    [TestCase("some textbother apprentice")]
    [TestCase("some textdang apprentice")]
    [TestCase("some textdrat apprentice")]
    [TestCase("some textbalderdash apprentice")]
    public void Title_Should_Not_Fail_IfWordContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            Title = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.Title);
        result.HasErrors.Should().BeFalse();
    }
}