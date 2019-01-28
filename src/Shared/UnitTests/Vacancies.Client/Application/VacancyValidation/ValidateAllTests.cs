using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation
{
    public class ValidateAllTests : VacancyValidationTestsBase
    {
        [Fact]
        public void ShouldNotCrashWhenValidatingAnEmptyVacancy()
        {
            var vacancy = new Vacancy();
            
            var result = Validator.Validate(vacancy, VacancyRuleSet.All);

            result.HasErrors.Should().BeTrue();
        }
    }
}
