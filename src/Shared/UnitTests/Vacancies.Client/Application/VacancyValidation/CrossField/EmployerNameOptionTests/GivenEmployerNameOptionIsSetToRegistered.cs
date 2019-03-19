using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField.EmployerNameOptionTests
{

    public class GivenSourceOriginIsProviderWeb : VacancyValidationTestsBase
    {
        [Fact]
        public void AndEmployerNameIsEmpty_ThenValidateUsingEmployerNameFlag()
        {
            var vacancy = new Vacancy()
            {
                EmployerName = string.Empty,
                EmployerNameOption = EmployerNameOption.RegisteredName,
                SourceOrigin = SourceOrigin.ProviderWeb
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerName | VacancyRuleSet.TradingName);

            
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be("4");
        }

        [Fact]
        public void ThenIgnoreLegalEntityName()
        {
            var vacancy = new Vacancy()
            {
                EmployerName = "test",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                SourceOrigin = SourceOrigin.ProviderWeb
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName |  VacancyRuleSet.EmployerName | VacancyRuleSet.LegalEntityName);
            result.HasErrors.Should().BeFalse();
        }
    }

    public class GivenSourceOriginIsEmployerWeb : VacancyValidationTestsBase
    {
        [Fact]
        public void ThenValidateUsingTradingNameFlag()
        {
            var vacancy = new Vacancy()
            {
                EmployerName = string.Empty,
                EmployerNameOption = EmployerNameOption.TradingName,
                SourceOrigin = SourceOrigin.EmployerWeb
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName |  VacancyRuleSet.EmployerName);
            
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be("401");
        }

        [Fact]
        public void ThenEmployerShouldNotBeValidatedForSpecialCharactersAndLength()        
        {
            var vacancy = new Vacancy()
            {
                EmployerName = "£$$%$%£$<>" + new string('a', 100),
                EmployerNameOption = EmployerNameOption.TradingName,
                SourceOrigin = SourceOrigin.EmployerWeb
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerName | VacancyRuleSet.TradingName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(2);
            result.Errors.Any(e => e.ErrorCode == "402").Should().BeTrue();
            result.Errors.Any(e => e.ErrorCode == "403").Should().BeTrue();
        }        
    }
}