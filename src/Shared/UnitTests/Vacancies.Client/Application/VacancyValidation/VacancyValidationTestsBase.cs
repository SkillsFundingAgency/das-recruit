using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation
{
    public abstract class VacancyValidationTestsBase
    {
        protected readonly Mock<IMinimumWageProvider> MockMinimumWageService;
        protected readonly Mock<IApprenticeshipProgrammeProvider> MockApprenticeshipProgrammeProvider;
        protected readonly Mock<IQualificationsProvider> MockQualificationsProvider;
        protected readonly IHtmlSanitizerService SanitizerService;

        protected VacancyValidationTestsBase()
        {
            MockMinimumWageService = new Mock<IMinimumWageProvider>();
            MockApprenticeshipProgrammeProvider = new Mock<IApprenticeshipProgrammeProvider>();
            MockQualificationsProvider = new Mock<IQualificationsProvider>();
            SanitizerService = new HtmlSanitizerService(new Mock<ILogger<HtmlSanitizerService>>().Object);
        }

        protected IEntityValidator<Vacancy, VacancyRuleSet> Validator
        {
            get
            {
                var timeProvider = new CurrentUtcTimeProvider();
                var fluentValidator = new FluentVacancyValidator(timeProvider, MockMinimumWageService.Object, MockApprenticeshipProgrammeProvider.Object, MockQualificationsProvider.Object, SanitizerService);
                return new EntityValidator<Vacancy, VacancyRuleSet>(fluentValidator);
            }
        }
    }
}
