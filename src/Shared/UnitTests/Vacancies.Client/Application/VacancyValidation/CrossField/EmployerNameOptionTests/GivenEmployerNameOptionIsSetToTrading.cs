using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField.EmployerNameOptionTests
{
    public class GivenEmployerNameOptionIsSetToTrading : VacancyValidationTestsBase
    {
        [Fact]
        public void WhenNameOptionIsTradingName_ShouldValidateTradingName()        
        {
            var vacancy = new Vacancy()
            {
                EmployerName = string.Empty,
                EmployerNameOption = EmployerNameOption.TradingName,
                SourceOrigin = SourceOrigin.EmployerWeb
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be("401");
        }

        [Fact]
        public void WhenNameSourceOriginIsEmployerWeb()
        {
            var vacancy = new Vacancy()
            {
                EmployerNameOption = EmployerNameOption.TradingName,
                SourceOrigin = SourceOrigin.EmployerWeb
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName | VacancyRuleSet.LegalEntityName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(2);
            result.Errors.Any(e => e.ErrorCode == "400").Should().BeTrue();
            result.Errors.Any(e => e.ErrorCode == "401").Should().BeTrue();
        }

    }
}