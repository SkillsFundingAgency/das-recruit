using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation;

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