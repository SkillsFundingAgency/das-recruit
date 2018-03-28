using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Moq;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation
{
    public abstract class VacancyValidationTestsBase
    {
        protected IEntityValidator<Vacancy, VacancyRuleSet> Validator;
        protected Mock<IGetApprenticeshipNationalMinimumWages> MockMinimumWageService;
        protected Mock<IQueryStoreReader> MockQueryStoreReader;

        protected VacancyValidationTestsBase()
        {
            var timeProvider = new CurrentTimeProvider();
            MockMinimumWageService = new Mock<IGetApprenticeshipNationalMinimumWages>();
            MockQueryStoreReader = new Mock<IQueryStoreReader>();

            var fluentValidator = new FluentVacancyValidator(timeProvider, MockMinimumWageService.Object, MockQueryStoreReader.Object);

            Validator = new EntityValidator<Vacancy, VacancyRuleSet>(fluentValidator);
        }
    }
}
