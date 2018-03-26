using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Moq;

namespace UnitTests.Application.VacancyValidation
{
    public abstract class VacancyValidationTestsBase
    {
        protected IEntityValidator<Vacancy, VacancyRuleSet> Validator;
        protected Mock<IGetApprenticeshipNationalMinimumWages> MockMinimumWageService;

        protected VacancyValidationTestsBase()
        {
            var timeProvider = new CurrentTimeProvider();
            MockMinimumWageService = new Mock<IGetApprenticeshipNationalMinimumWages>();

            var fluentValidator = new FluentVacancyValidator(timeProvider, MockMinimumWageService.Object);

            Validator = new EntityValidator<Vacancy, VacancyRuleSet>(fluentValidator);
        }
    }
}
