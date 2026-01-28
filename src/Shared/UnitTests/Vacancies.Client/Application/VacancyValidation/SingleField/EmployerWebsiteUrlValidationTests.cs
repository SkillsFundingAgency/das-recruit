using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class EmployerWebsiteUrlValidationTests : VacancyValidationTestsBase
{
    [TestCase(null)]
    [TestCase("")]
    
    [TestCase("http://www.company.com")]
    [TestCase("https://www.company.com")]
    public void NoErrorsWhenEmployerWebsiteUrlIsValid(string url)
    {
        var vacancy = new Vacancy
        {
            EmployerWebsiteUrl = url
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerWebsiteUrl);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }
        
    [Test]
    public void EmployerWebsiteUrlMustBe100CharactersOrLess()
    {
        var vacancy = new Vacancy
        {
            EmployerWebsiteUrl = "http://www.company.com".PadRight(101, 'w')
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerWebsiteUrl);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerWebsiteUrl));
        result.Errors[0].ErrorCode.Should().Be("84");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerWebsiteUrl);
    }

    [TestCase("invalid url")]
    [TestCase("company")]
    [TestCase("domain.com ?term=query")]
    [TestCase(".com")]
    [TestCase(".org.uk")]
    [TestCase(",com")]
    [TestCase("company.com")]
    [TestCase("www.company.com")]
    [TestCase("/apply")]
    [TestCase("/apply?source=foo")]
    [TestCase("/apply.aspx")]
    public void EmployerWebsiteUrlMustBeAValidWebAddress(string invalidUrl)
    {
        var vacancy = new Vacancy
        {
            EmployerWebsiteUrl = invalidUrl
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerWebsiteUrl);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerWebsiteUrl));
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerWebsiteUrl);
    }
}