using System;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Moq;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.VacancyAnalyticsOrchestratorTests
{
    public class VacancyAnalyticsOrchestratorTests
    {
        private Fixture _fixture;
        private Mock<IRecruitVacancyClient> _vacancyClient;
        private Mock<ProviderRecruitSystemConfiguration> _systemConfig;
        private IVacancyAnalyticsOrchestrator _orchestrator;
        private Guid _vacancyId;
        private long _ukprn;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _vacancyClient = new Mock<IRecruitVacancyClient>();
            _systemConfig = new Mock<ProviderRecruitSystemConfiguration>();
            _orchestrator = new VacancyAnalyticsOrchestrator(_vacancyClient.Object, _systemConfig.Object);
            _vacancyId = Guid.NewGuid();
            _ukprn = 10000034;
        }

        [Test]
        public async Task GetVacancyAnalytics_VacancyAnalyticsFound_ReturnsCorrectVacancyAnalytics()
        {
            // Arrange
            var vacancyRouteModel = _fixture.Build<VacancyRouteModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();
            var vacancy = _fixture.Build<Vacancy>()
                .With(x => x.Id, _vacancyId)
                .Create();
            var vacancyAnalyticsSummary = _fixture.Create<VacancyAnalyticsSummary>();

            _vacancyClient.Setup(x => x.GetVacancyAsync((Guid)vacancyRouteModel.VacancyId))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyAnalyticsSummaryAsync((long)vacancy.VacancyReference))
                .ReturnsAsync(vacancyAnalyticsSummary);

            // Act
            var viewModel = await _orchestrator.GetVacancyAnalytics(vacancyRouteModel);

            // Assert
            Assert.That(_ukprn, Is.EqualTo(viewModel.Ukprn));
            Assert.That(_vacancyId, Is.EqualTo(viewModel.VacancyId));
            Assert.That(viewModel.AnalyticsSummary, Is.Not.Null);
            Assert.That(vacancyAnalyticsSummary.NoOfApprenticeshipSearches, Is.EqualTo(viewModel.AnalyticsSummary.NoOfTimesAppearedInSearch));
            Assert.That(vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreated, Is.EqualTo(viewModel.AnalyticsSummary.NoOfApplicationsStarted));
            Assert.That(vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmitted, Is.EqualTo(viewModel.AnalyticsSummary.NoOfApplicationsSubmitted));
            Assert.That(vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViews, Is.EqualTo(viewModel.AnalyticsSummary.NoOfTimesViewed));
            Assert.That((vacancyAnalyticsSummary.NoOfApprenticeshipSearchesSevenDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesSixDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesFiveDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesFourDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesThreeDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesTwoDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesOneDayAgo), Is.EqualTo(viewModel.AnalyticsSummary.NoOfTimesAppearedInSearchOverLastSevenDays));
            Assert.That((vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedSevenDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedSixDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedFiveDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedFourDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedThreeDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedTwoDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedOneDayAgo), Is.EqualTo(viewModel.AnalyticsSummary.NoOfApplicationsStartedOverLastSevenDays));
            Assert.That((vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsOneDayAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsTwoDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsThreeDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsFourDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsFiveDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsSixDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsSevenDaysAgo), Is.EqualTo(viewModel.AnalyticsSummary.NoOfTimesViewedOverLastSevenDays));
        }

        [Test]
        public async Task GetVacancyAnalytics_NoVacancyAnalyticsFound_ReturnsVacancyAnalyticsWithZeroValues()
        {
            // Arrange
            var vacancyRouteModel = _fixture.Build<VacancyRouteModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();
            var vacancy = _fixture.Build<Vacancy>()
                .With(x => x.Id, _vacancyId)
                .Create();

            _vacancyClient.Setup(x => x.GetVacancyAsync((Guid)vacancyRouteModel.VacancyId))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyAnalyticsSummaryAsync((long)vacancy.VacancyReference))
                .ReturnsAsync(new VacancyAnalyticsSummary());

            // Act
            var viewModel = await _orchestrator.GetVacancyAnalytics(vacancyRouteModel);

            // Assert
            Assert.That(_ukprn, Is.EqualTo(viewModel.Ukprn));
            Assert.That(_vacancyId, Is.EqualTo(viewModel.VacancyId));
            Assert.That(viewModel.AnalyticsSummary, Is.Not.Null);
            Assert.That(0, Is.EqualTo(viewModel.AnalyticsSummary.NoOfTimesAppearedInSearch));
            Assert.That(0, Is.EqualTo(viewModel.AnalyticsSummary.NoOfApplicationsStarted));
            Assert.That(0, Is.EqualTo(viewModel.AnalyticsSummary.NoOfApplicationsSubmitted));
            Assert.That(0, Is.EqualTo(viewModel.AnalyticsSummary.NoOfTimesViewed));
            Assert.That(0, Is.EqualTo(viewModel.AnalyticsSummary.NoOfTimesAppearedInSearchOverLastSevenDays));
            Assert.That(0, Is.EqualTo(viewModel.AnalyticsSummary.NoOfApplicationsStartedOverLastSevenDays));
            Assert.That(0, Is.EqualTo(viewModel.AnalyticsSummary.NoOfTimesViewedOverLastSevenDays));
        }
    }
}
