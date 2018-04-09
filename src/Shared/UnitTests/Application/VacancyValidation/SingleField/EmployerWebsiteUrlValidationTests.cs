using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public class EmployerWebsiteUrlValidationTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("http://www.company.com")]
        [InlineData("https://www.company.com")]
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
        
        [Fact]
        public void EmployerWebsiteUrlMustBe200CharactersOrLess()
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

        [Fact]
        public void EmployerWebsiteUrlMustBeAValidWebAddress()
        {
            var vacancy = new Vacancy
            {
                EmployerWebsiteUrl = "ftp://www.company.com"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerWebsiteUrl);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerWebsiteUrl));
            result.Errors[0].ErrorCode.Should().Be("82");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerWebsiteUrl);
        }
    }
}