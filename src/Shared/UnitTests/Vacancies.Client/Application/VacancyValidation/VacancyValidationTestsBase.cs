using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Rules.VacancyRules;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation
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
        protected readonly Mock<ITrainingProviderService> MockTrainingProviderService;
        protected ITimeProvider TimeProvider;
        protected readonly Mock<IFeature> Feature;

        protected VacancyValidationTestsBase()
        {
            MockMinimumWageService = new Mock<IMinimumWageProvider>();
            MockApprenticeshipProgrammeProvider = new Mock<IApprenticeshipProgrammeProvider>();
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammeAsync("123", null))
                .ReturnsAsync(new ApprenticeshipProgramme
                {
                    IsActive = true,
                    Id = "123",
                });
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammeAsync("123", 10000000))
                .ReturnsAsync(new ApprenticeshipProgramme
                {
                    IsActive = true,
                    Id = "123",
                });
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammeAsync("000", null))
                .ReturnsAsync(new ApprenticeshipProgramme
                {
                    Id = "abc",
                    IsActive = false
                });
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false, null))
                .ReturnsAsync(new List<ApprenticeshipProgramme>
                {
                    new()
                    {
                        Id = "123",
                        IsActive = true
                    }
                });
            MockQualificationsProvider = new Mock<IQualificationsProvider>();
            SanitizerService = new HtmlSanitizerService(new Mock<ILogger<HtmlSanitizerService>>().Object);
            MockTrainingProviderSummaryProvider = new Mock<ITrainingProviderSummaryProvider>();
            MockTrainingProviderSummaryProvider.Setup(x => x.GetAsync(10000000))
                .ReturnsAsync(new TrainingProviderSummary{IsTrainingProviderMainOrEmployerProfile = true});
            MockTrainingProviderSummaryProvider.Setup(x => x.GetAsync(10000000))
                .ReturnsAsync(new TrainingProviderSummary());
            MockBlockedOrganisationRepo = new Mock<IBlockedOrganisationQuery>();
            MockProfanityListProvider = new TestProfanityListProvider();
            MockProviderRelationshipsService = new Mock<IProviderRelationshipsService>();
            MockTrainingProviderService = new Mock<ITrainingProviderService>();
            MockTrainingProviderService.Setup(x => x.GetCourseProviders(123)).ReturnsAsync(new List<TrainingProviderSummary>()
            {
                new()
                {
                    Ukprn = 10000000,
                }
            });
            TimeProvider = new CurrentUtcTimeProvider();
            Feature = new Mock<IFeature>();
        }

        protected IEntityValidator<Vacancy, VacancyRuleSet> Validator
        {
            get
            {
                var fluentValidator = new FluentVacancyValidator(TimeProvider, MockMinimumWageService.Object, 
                    MockApprenticeshipProgrammeProvider.Object, MockQualificationsProvider.Object, SanitizerService, 
                    MockTrainingProviderSummaryProvider.Object, MockTrainingProviderService.Object, MockBlockedOrganisationRepo.Object,
                    MockProfanityListProvider, MockProviderRelationshipsService.Object);
                return new EntityValidator<Vacancy, VacancyRuleSet>(fluentValidator);
            }
        }
    }
}
