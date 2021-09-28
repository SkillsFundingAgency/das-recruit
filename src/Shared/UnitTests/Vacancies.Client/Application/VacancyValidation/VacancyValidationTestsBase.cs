using Esfa.Recruit.UnitTests.Vacancies.Client.Application.Rules.VacancyRules;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;
using Moq;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation
{
    public abstract class VacancyValidationTestsBase
    {
        protected readonly Mock<IMinimumWageProvider> MockMinimumWageService;
        protected readonly Mock<IApprenticeshipProgrammeProvider> MockApprenticeshipProgrammeProvider;
        protected readonly Mock<IQualificationsProvider> MockQualificationsProvider;
        protected readonly IHtmlSanitizerService SanitizerService;
        protected readonly Mock<ITrainingProviderSummaryProvider> MockTrainingProviderSummaryProvider;
        protected readonly Mock<IBlockedOrganisationQuery> MockBlockedOrganisationRepo;
        protected readonly TestProfanityListProvider MockProfanityListProvider;
        protected readonly Mock<IProviderRelationshipsService> MockProviderRelationshipsService;

        protected VacancyValidationTestsBase()
        {
            MockMinimumWageService = new Mock<IMinimumWageProvider>();
            MockApprenticeshipProgrammeProvider = new Mock<IApprenticeshipProgrammeProvider>();
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammeAsync("11"))
                .ReturnsAsync(new ApprenticeshipProgramme());
            MockQualificationsProvider = new Mock<IQualificationsProvider>();
            SanitizerService = new HtmlSanitizerService(new Mock<ILogger<HtmlSanitizerService>>().Object);
            MockTrainingProviderSummaryProvider = new Mock<ITrainingProviderSummaryProvider>();
            MockBlockedOrganisationRepo = new Mock<IBlockedOrganisationQuery>();
            MockProfanityListProvider = new TestProfanityListProvider();
            MockProviderRelationshipsService = new Mock<IProviderRelationshipsService>();
        }

        protected IEntityValidator<Vacancy, VacancyRuleSet> Validator
        {
            get
            {
                var timeProvider = new CurrentUtcTimeProvider();
                var fluentValidator = new FluentVacancyValidator(timeProvider, MockMinimumWageService.Object, 
                    MockApprenticeshipProgrammeProvider.Object, MockQualificationsProvider.Object, SanitizerService, 
                    MockTrainingProviderSummaryProvider.Object, MockBlockedOrganisationRepo.Object,
                    MockProfanityListProvider, MockProviderRelationshipsService.Object);
                return new EntityValidator<Vacancy, VacancyRuleSet>(fluentValidator);
            }
        }
    }
}
