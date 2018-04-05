using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation
{
    public abstract class VacancyValidationTestsBase
    {
        protected readonly Mock<IGetMinimumWages> MockMinimumWageService;
        protected readonly Mock<IQueryStoreReader> MockQueryStoreReader;
        protected readonly Mock<IOptions<QualificationsConfiguration>> MockQualificationConfiguration;

        protected VacancyValidationTestsBase()
        {
            MockMinimumWageService = new Mock<IGetMinimumWages>();
            MockQueryStoreReader = new Mock<IQueryStoreReader>();
            MockQualificationConfiguration = new Mock<IOptions<QualificationsConfiguration>>();
        }

        protected IEntityValidator<Vacancy, VacancyRuleSet> Validator
        {
            get
            {
                var timeProvider = new CurrentUtcTimeProvider();
                var fluentValidator = new FluentVacancyValidator(timeProvider, MockMinimumWageService.Object, MockQueryStoreReader.Object, MockQualificationConfiguration.Object);
                return new EntityValidator<Vacancy, VacancyRuleSet>(fluentValidator);
            }
        }
    }
}
