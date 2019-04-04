using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class EmployerNameTests : VacancyValidationTestsBase
    {
        [Fact]
        public void ProviderWeb_RegisteredName_ShouldValidateEmpty()
        {
            var vacancy = new Vacancy() {
                EmployerName = string.Empty
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be("4");
        }

        [Fact]
        public void TradingName_ShouldValidateEmpty()
        {
            var vacancy = new Vacancy() {
                EmployerName = string.Empty,
                EmployerNameOption = EmployerNameOption.TradingName
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be("401");
        }

        [Fact]
        public void TradingName_ShouldValidateSpecialCharactersAndLength()
        {
            var vacancy = new Vacancy() {
                EmployerName = "�$$%$%�$<>" + new string('a', 100),
                EmployerNameOption = EmployerNameOption.TradingName
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(2);
            result.Errors.Any(e => e.ErrorCode == "402").Should().BeTrue();
            result.Errors.Any(e => e.ErrorCode == "403").Should().BeTrue();
        }

        [Fact]
        public void EmployerWeb_Anonymous_ShouldValidateEmpty()        
        {
            var vacancy = new Vacancy()
            {
                EmployerName = string.Empty,
                EmployerNameOption = EmployerNameOption.Anonymous,
                SourceOrigin = SourceOrigin.EmployerWeb,
                AnonymousReason = "a valid reason"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be("401");
        }

        

        [Fact]
        public void EmployerWeb_Anonymous_ShouldValidateSpecialCharactersAndLength()
        {
            var vacancy = new Vacancy() {
                EmployerName = "�$$%$%�$<>" + new string('a', 100),
                EmployerNameOption = EmployerNameOption.Anonymous,
                SourceOrigin = SourceOrigin.EmployerWeb,
                AnonymousReason = "a valid reason"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(2);
            result.Errors.Any(e => e.ErrorCode == "402").Should().BeTrue();
            result.Errors.Any(e => e.ErrorCode == "403").Should().BeTrue();
        }
    }
}