using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Dashboard
{
    public class GetDashboardViewModelAsyncTests
    {
        private const long Ukprn = 12345678;
        private const string UserId = "user id";

        private readonly DateTime _today = DateTime.Parse("2019-09-18");
        private readonly VacancyUser _user = new() {UserId = UserId,  Ukprn = Ukprn };
        private Mock<IRecruitVacancyClient> _clientMock;

        [Fact]
        public async Task WhenHasVacancies_ShouldReturnViewModelAsync()
        {
            var fixture = new Fixture();
            var dashboardProjection = fixture.Create<ProviderDashboardSummary>();

            var orchestrator = GetSut(dashboardProjection);

            var actualDashboard = await orchestrator.GetDashboardViewModelAsync(_user);

            actualDashboard.HasAnyVacancies.Should().BeTrue();
            actualDashboard.Alerts.Should().NotBeNull();
            actualDashboard.Ukprn.Should().Be(Ukprn);
        }

        [Fact]
        public async Task WhenHasNoVacancies_ShouldReturnViewModelAsync()
        {
            var orchestrator = GetSut(new ProviderDashboardSummary());

            var actualDashboard = await orchestrator.GetDashboardViewModelAsync(_user);

            actualDashboard.HasAnyVacancies.Should().BeFalse();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(0);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(0);
            actualDashboard.Alerts.Should().NotBeNull();
            actualDashboard.Ukprn.Should().Be(Ukprn);
        }

        [Fact]
        public async Task Then_Upserts_Provider_User()
        {
            var orchestrator = GetSut(new ProviderDashboardSummary());

            await orchestrator.GetDashboardViewModelAsync(_user);
            
            _clientMock.Verify(x=>x.UserSignedInAsync(_user, UserType.Provider), Times.Once);
        }

        private DashboardOrchestrator GetSut(ProviderDashboardSummary dashboardProjection)
        {
            var timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock.Setup(t => t.Today).Returns(_today);

            var vacancyClientMock = new Mock<IProviderVacancyClient>();
            vacancyClientMock.Setup(c => c.GetDashboardSummary(Ukprn, UserId))
                .ReturnsAsync(dashboardProjection);

            var userDetails = new User();

            _clientMock = new Mock<IRecruitVacancyClient>();
            
            _clientMock.Setup(c => c.GetUsersDetailsByDfEUserId(It.IsAny<string>()))
                .ReturnsAsync(userDetails);

            var alertsViewModel = new AlertsViewModel(null, null, Ukprn);
            var alertsFactoryMock = new Mock<IProviderAlertsViewModelFactory>();
            alertsFactoryMock.Setup(a => a.Create(userDetails))
                .ReturnsAsync(alertsViewModel);

            return new DashboardOrchestrator(vacancyClientMock.Object, _clientMock.Object);
        }
    }
}