using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class ExternalApplicationUrlValidationTests : VacancyValidationTestsBase
{
    [TestCase("http://www.applyhere.com")]
    [TestCase("https://www.applyhere.com")]
    [TestCase("https://www.applyhere.com/apply")]
    [TestCase("https://www.applyhere.com/apply?source=foo")]
    [TestCase("https://www.applyhere.com/apply.aspx?source=foo")]
    public void NoErrorsWhenExternalApplicationUrlIsValid(string url)
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationUrl = url
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [TestCase(null)]
    [TestCase("")]
    public void ExternalApplicationUrlMustHaveAValue(string url)
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationUrl = url
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationUrl));
        result.Errors[0].ErrorCode.Should().Be("85");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationMethod);
    }

    [Test]
    public void ExternalApplicationUrlMustBe2000CharactersOrLess()
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationUrl = "http://www.applyhere.com".PadRight(2001, 'w')
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationUrl));
        result.Errors[0].ErrorCode.Should().Be("84");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationMethod);
    }

    [TestCase("Invalid Url")]
    [TestCase("applyhere")]
    [TestCase("domain.com ?term=query")]
    [TestCase(".com")]
    [TestCase(".org.uk")]
    [TestCase(",com")]
    [TestCase("applyhere.com")]
    [TestCase("www.applyhere.com")]
    [TestCase("applyhere.com#anchor")]
    [TestCase("applyhere.com?term=query")]
    public void ExternalApplicationUrlMustBeAValidWebAddress(string invalidUrl)
    {
        var vacancy = new Vacancy
        {
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationUrl = invalidUrl
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationMethod);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationUrl));
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationMethod);
    }
}