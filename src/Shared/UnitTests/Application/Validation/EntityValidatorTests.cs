using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation
{
    public class EntityValidatorTests
    {
        private EntityValidator<Vacancy, VacancyRuleSet> _validator;

        public EntityValidatorTests()
        {
            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator(new CurrentTimeProvider()));

        }

        [Fact]
        public void ValidateReturnsFailWhenValidationFails()
        {
            var invalidVacancy = new Vacancy();

            var result = _validator.Validate(invalidVacancy, VacancyRuleSet.All);

            result.HasErrors.Should().BeTrue();
        }

        [Fact]
        public void ValidateReturnsListOfValidationErrorWhenValidationFails()
        {
            var invalidVacancy = new Vacancy();

            var result = _validator.Validate(invalidVacancy, VacancyRuleSet.All);

            result.Errors.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void ValidateReturnsNoErrorWhenValidationPasses()
        {
            var invalidVacancy = new Vacancy { Title = "Valid Title" };

            var result = _validator.Validate(invalidVacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }
    }
}