using AutoFixture;
using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators
{
    public class VacancyAnalyticsOrchestratorTests
    {
        private Fixture _fixture;
        private Mock<IRecruitVacancyClient> _vacancyClient;
        private Mock<IUtility> _utility;
        private Mock<EmployerRecruitSystemConfiguration> _systemConfig;    
        private IVacancyAnalyticsOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _vacancyClient = new Mock<IRecruitVacancyClient>();
            _utility = new Mock<IUtility>();
            _systemConfig = new Mock<EmployerRecruitSystemConfiguration>();
            _orchestrator = new VacancyAnalyticsOrchestrator(_vacancyClient.Object, _systemConfig.Object, _utility.Object);
        }

        [Test]
        public async Task GetVacancyAnalytics_ReturnsCorrectVacancyAnalytics()
        {
            // Arrange
            var vacancyRouteModel = _fixture.Create<VacancyRouteModel>();
            var vacancy = _fixture.Create<Vacancy>();
            var vacancyAnalyticsSummary = _fixture.Create<VacancyAnalyticsSummary>();

            _vacancyClient.Setup(x => x.GetVacancyAsync(vacancyRouteModel.VacancyId))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyAnalyticsSummaryAsync((long)vacancy.VacancyReference))
                .ReturnsAsync(vacancyAnalyticsSummary);

            // Act
            var viewModel = await _orchestrator.GetVacancyAnalytics(vacancyRouteModel);

            // Assert
            Assert.IsNotNull(viewModel.AnalyticsSummary);
            Assert.AreEqual(vacancyAnalyticsSummary.NoOfApprenticeshipSearches, viewModel.AnalyticsSummary.NoOfTimesAppearedInSearch);
            Assert.AreEqual(vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreated, viewModel.AnalyticsSummary.NoOfApplicationsStarted);
            Assert.AreEqual(vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmitted, viewModel.AnalyticsSummary.NoOfApplicationsSubmitted);
            Assert.AreEqual(vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViews, viewModel.AnalyticsSummary.NoOfTimesViewed);
            Assert.AreEqual((vacancyAnalyticsSummary.NoOfApprenticeshipSearchesSevenDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesSixDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesFiveDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesFourDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesThreeDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesTwoDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesOneDayAgo), viewModel.AnalyticsSummary.NoOfTimesAppearedInSearchOverLastSevenDays);
            Assert.AreEqual((vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedSevenDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedSixDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedFiveDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedFourDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedThreeDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedTwoDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedOneDayAgo), viewModel.AnalyticsSummary.NoOfApplicationsStartedOverLastSevenDays);
            Assert.AreEqual((vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsOneDayAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsTwoDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsThreeDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsFourDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsFiveDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsSixDaysAgo
                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsSevenDaysAgo), viewModel.AnalyticsSummary.NoOfTimesViewedOverLastSevenDays);
        }

        [Test]
        public async Task GetVacancyAnalytics_NoVacancyAnalytics_ReturnsVacancyAnalyticsWithZeroValues()
        {
            // Arrange
            var vacancyRouteModel = _fixture.Create<VacancyRouteModel>();
            var vacancy = _fixture.Create<Vacancy>();

            _vacancyClient.Setup(x => x.GetVacancyAsync(vacancyRouteModel.VacancyId))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyAnalyticsSummaryAsync((long)vacancy.VacancyReference))
                .ReturnsAsync(new VacancyAnalyticsSummary());

            // Act
            var viewModel = await _orchestrator.GetVacancyAnalytics(vacancyRouteModel);

            // Assert
            Assert.IsNotNull(viewModel.AnalyticsSummary);
            Assert.AreEqual(0, viewModel.AnalyticsSummary.NoOfTimesAppearedInSearch);
            Assert.AreEqual(0, viewModel.AnalyticsSummary.NoOfApplicationsStarted);
            Assert.AreEqual(0, viewModel.AnalyticsSummary.NoOfApplicationsSubmitted);
            Assert.AreEqual(0, viewModel.AnalyticsSummary.NoOfTimesViewed);
            Assert.AreEqual(0, viewModel.AnalyticsSummary.NoOfTimesAppearedInSearchOverLastSevenDays);
            Assert.AreEqual(0, viewModel.AnalyticsSummary.NoOfApplicationsStartedOverLastSevenDays);
            Assert.AreEqual(0, viewModel.AnalyticsSummary.NoOfTimesViewedOverLastSevenDays);
        }
    }
}
