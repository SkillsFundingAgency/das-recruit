using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Wages;
using Microsoft.Extensions.Options;
using Moq;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation
{
    public abstract class VacancyValidationTestsBase
    {
        protected readonly Mock<IGetMinimumWages> MockMinimumWageService;
        protected readonly Mock<IApprenticeshipProgrammeProvider> MockApprenticeshipProgrammeProvider;
        protected readonly Mock<IOptions<QualificationsConfiguration>> MockQualificationConfiguration;

        protected VacancyValidationTestsBase()
        {
            MockMinimumWageService = new Mock<IGetMinimumWages>();
            MockApprenticeshipProgrammeProvider = new Mock<IApprenticeshipProgrammeProvider>();
            MockQualificationConfiguration = new Mock<IOptions<QualificationsConfiguration>>();
        }

        protected IEntityValidator<Vacancy, VacancyRuleSet> Validator
        {
            get
            {
                var timeProvider = new CurrentUtcTimeProvider();
                var fluentValidator = new FluentVacancyValidator(timeProvider, MockMinimumWageService.Object, MockApprenticeshipProgrammeProvider.Object, MockQualificationConfiguration.Object);
                return new EntityValidator<Vacancy, VacancyRuleSet>(fluentValidator);
            }
        }
    }
}
