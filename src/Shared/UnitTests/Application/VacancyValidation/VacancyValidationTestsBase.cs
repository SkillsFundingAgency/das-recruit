using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Moq;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation
{
    public abstract class VacancyValidationTestsBase
    {
        protected readonly IEntityValidator<Vacancy, VacancyRuleSet> Validator;
        protected readonly Mock<IGetApprenticeNationalMinimumWages> MockMinimumWageService;
        protected readonly Mock<IQueryStoreReader> MockQueryStoreReader;

        protected VacancyValidationTestsBase()
        {
            var timeProvider = new CurrentUtcTimeProvider();
            MockMinimumWageService = new Mock<IGetApprenticeNationalMinimumWages>();
            MockQueryStoreReader = new Mock<IQueryStoreReader>();

            var fluentValidator = new FluentVacancyValidator(timeProvider, MockMinimumWageService.Object, MockQueryStoreReader.Object);

            Validator = new EntityValidator<Vacancy, VacancyRuleSet>(fluentValidator);
        }
    }
}
