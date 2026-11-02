using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class ThroughFaaApplicationMethodValidationTests : VacancyValidationTestsBase
{
    [Fact]
    public void ApplicationMethodCannotBeNull()
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = null
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorCode.Should().Be("85");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationMethod);
    }

    [Fact]
    public void NoErrorsWhenApplicationMethodIsThroughFaa()
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void WhenApplicationMethodIsThroughFaaExternalApplicationUrlShouldBeEmpty()
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
            ApplicationUrl = "http://www.apply.com",
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationUrl));
        result.Errors[0].ErrorCode.Should().Be("86");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationMethod);
    }

    [Fact]
    public void WhenApplicationMethodIsThroughFaaExternalApplicationInstructionsShouldBeEmpty()
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
            ApplicationInstructions = "you must do this"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationInstructions));
        result.Errors[0].ErrorCode.Should().Be("89");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationMethod);
    }

    [Theory]
    [InlineData("some text bother")]
    [InlineData("some text dang")]
    [InlineData("some text drat")]
    [InlineData("some text balderdash")]
    public void WhenApplicationMethodIsThroughFaaExternalApplicationInstructions_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationInstructions = freeText,
            ApplicationUrl = "http://www.apply.com"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationInstructions));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("612");
    }

    [Theory]
    [InlineData("some textbother")]
    [InlineData("some textdang")]
    [InlineData("some textdrat")]
    [InlineData("some textbalderdash")]
    public void WhenApplicationMethodIsThroughFaaExternalApplicationInstructions_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationInstructions = freeText,
            ApplicationUrl = "http://www.apply.com"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);
        result.HasErrors.Should().BeFalse();
    }
}