using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using UnitTests.Application.VacancyValidation;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation
{
    public class EntityValidatorTests : VacancyValidationTestsBase
    {
        [Fact]
        public void ValidateReturnsFailWhenValidationFails()
        {
            var invalidVacancy = new Vacancy
            {
                Location = new Address(),
                Wage = new Wage(),
                Programme = new Programme()
            };

            var result = Validator.Validate(invalidVacancy, VacancyRuleSet.All);

            result.HasErrors.Should().BeTrue();
        }

        [Fact]
        public void ValidateReturnsListOfValidationErrorWhenValidationFails()
        {
            var invalidVacancy = new Vacancy
            {
                Location = new Address(),
                Wage = new Wage(),
                Programme = new Programme()
            };

            var result = Validator.Validate(invalidVacancy, VacancyRuleSet.All);

            result.Errors.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void ValidateReturnsNoErrorWhenValidationPasses()
        {
            var validVacancy = new Vacancy
            {
                Title = "Valid Title",
                Location = new Address(),
                Wage = new Wage(),
                Programme = new Programme()
            };

            var result = Validator.Validate(validVacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }
    }
}