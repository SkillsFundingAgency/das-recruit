using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators
{
    public class VacancyAnalyticsOrchestratorTests
    {
        private Fixture _fixture;
        private Mock<IRecruitVacancyClient> _vacancyClient;
        private Mock<IUtility> _utility;
        private IVacancyAnalyticsOrchestrator _orchestrator;
        private Guid _vacancyId;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _vacancyClient = new Mock<IRecruitVacancyClient>();
            _utility = new Mock<IUtility>();
            _orchestrator = new VacancyAnalyticsOrchestrator(_vacancyClient.Object, _utility.Object);
            _vacancyId = Guid.NewGuid();
        }

        [Test]
        public async Task GetVacancyAnalytics_ReturnsCorrectVacancyAnalytics()
        {
            // Arrange
            var vacancyRouteModel = _fixture.Build<VacancyRouteModel>()
                .With(x => x.VacancyId, _vacancyId)
                .Create();
            var vacancy = _fixture.Build<Vacancy>()
                .With(x => x.Id, _vacancyId)
                .Create();
            var vacancyAnalyticsSummary = _fixture.Create<VacancyAnalyticsSummary>();

            _vacancyClient.Setup(x => x.GetVacancyAsync(vacancyRouteModel.VacancyId))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyAnalyticsSummaryAsync((long)vacancy.VacancyReference))
                .ReturnsAsync(vacancyAnalyticsSummary);

            // Act
            var viewModel = await _orchestrator.GetVacancyAnalytics(vacancyRouteModel);

            // Assert
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
        public async Task GetVacancyAnalytics_NoVacancyAnalytics_ReturnsVacancyAnalyticsWithZeroValues()
        {
            // Arrange
            var vacancyRouteModel = _fixture.Build<VacancyRouteModel>()
                .With(x => x.VacancyId, _vacancyId)
                .Create();
            var vacancy = _fixture.Build<Vacancy>()
                .With(x => x.Id, _vacancyId)
                .Create();

            _vacancyClient.Setup(x => x.GetVacancyAsync(vacancyRouteModel.VacancyId))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyAnalyticsSummaryAsync((long)vacancy.VacancyReference))
                .ReturnsAsync(new VacancyAnalyticsSummary());

            // Act
            var viewModel = await _orchestrator.GetVacancyAnalytics(vacancyRouteModel);

            // Assert
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
