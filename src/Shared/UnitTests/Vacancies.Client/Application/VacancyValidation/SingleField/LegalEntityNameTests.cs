using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField.EmployerNameOptionTests
{
    public class LegalEntityNameTests : VacancyValidationTestsBase
    {
        [Fact]
        public void ShouldValidateEmpty()
        {
            var vacancy = new Vacancy() {
                LegalEntityName = null,
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.LegalEntityName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.LegalEntityName));
            result.Errors[0].ErrorCode.Should().Be("400");
        }
    }
}