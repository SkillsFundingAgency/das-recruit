using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class ExternalApplicationUrlValidationTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData("applyhere.com")]
        [InlineData("www.applyhere.com")]
        [InlineData("http://www.applyhere.com")]
        [InlineData("https://www.applyhere.com")]
        [InlineData("applyhere.com#anchor")]
        [InlineData("applyhere.com?term=query")]
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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
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

        [Fact]
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

        [Theory]
        [InlineData("Invalid Url")]
        [InlineData("applyhere")]
        [InlineData("domain.com ?term=query")]
        [InlineData(".com")]
        [InlineData(".org.uk")]
        [InlineData(",com")]
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
            result.Errors[0].ErrorCode.Should().Be("86");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationMethod);
        }
    }
}