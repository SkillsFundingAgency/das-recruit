using Esfa.Recruit.UnitTests.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class ExternalApplicationInstructionsValidationTests : VacancyValidationTestsBase
{
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("You can apply online or through post.")]
    public void NoErrorsWhenExternalApplicationInstructionsIsValid(string instructions)
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationUrl = "http://www.apply.com",
            ApplicationInstructions = instructions
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Test]
    public void ExternalApplicationInstructionsMustBe500CharactersOrLess()
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationUrl = "http://www.apply.com",
            ApplicationInstructions = "instructions".PadRight(501, 'w')
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationInstructions));
        result.Errors[0].ErrorCode.Should().Be("88");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationMethod);
    }

    [Test]
    [TestCaseSource(typeof(TestData), nameof(TestData.BlacklistedCharacters))]
    public void ExternalApplicationInstructionsMustNotContainInvalidCharacters(char invalidChar)
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationUrl = "http://www.apply.com",
            ApplicationInstructions = invalidChar.ToString()
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationInstructions));
        result.Errors[0].ErrorCode.Should().Be("89");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationMethod);
    }
}