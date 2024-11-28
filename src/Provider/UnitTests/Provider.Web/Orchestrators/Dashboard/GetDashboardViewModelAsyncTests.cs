using System;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Dashboard
{
    public class GetDashboardViewModelAsyncTests
    {
        private const long Ukprn = 12345678;
        private const string UserId = "user id";

        private readonly DateTime _today = DateTime.Parse("2019-09-18");
        private readonly VacancyUser _user = new() {UserId = UserId,  Ukprn = Ukprn };
        private Mock<IProviderRelationshipsService> _permissionServiceMock;
        private Mock<IRecruitVacancyClient> _clientMock;

        [Theory]
        [InlineData(VacancyType.Apprenticeship)]
        public async Task WhenHasVacancies_ShouldReturnViewModelAsync(VacancyType vacancyType)
        {
            var fixture = new Fixture();
            var dashboardProjection = fixture.Create<ProviderDashboardSummary>();

            var orch = GetSut(dashboardProjection);

            var actualDashboard = await orch.GetDashboardViewModelAsync(_user);

            actualDashboard.HasAnyVacancies.Should().BeTrue();
            actualDashboard.Alerts.Should().NotBeNull();
            actualDashboard.Ukprn.Should().Be(Ukprn);
        }

        [Theory]
        [InlineData(VacancyType.Apprenticeship)]
        public async Task WhenHasNoVacancies_ShouldReturnViewModelAsync(VacancyType vacancyType)
        {
            var orch = GetSut(new ProviderDashboardSummary());

            var actualDashboard = await orch.GetDashboardViewModelAsync(_user);

            actualDashboard.HasAnyVacancies.Should().BeFalse();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(0);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(0);
            actualDashboard.Alerts.Should().NotBeNull();
            actualDashboard.Ukprn.Should().Be(Ukprn);
        }

        [Theory]
        [InlineData(VacancyType.Apprenticeship)]
        public async Task Then_Checks_For_CorrectPermission_BasedOn_Vacancy_Type(VacancyType vacancyType)
        {
            var orch = GetSut(new ProviderDashboardSummary());

            var actual = await orch.GetDashboardViewModelAsync(_user);

            _permissionServiceMock.Verify(x => x.CheckProviderHasPermissions(Ukprn, OperationType.RecruitmentRequiresReview));
            actual.HasEmployerReviewPermission.Should().BeTrue();
        }

        [Fact]
        public async Task Then_Upserts_Provider_User()
        {
            var orch = GetSut(new ProviderDashboardSummary());

            await orch.GetDashboardViewModelAsync(_user);
            
            _clientMock.Verify(x=>x.UserSignedInAsync(_user, UserType.Provider), Times.Once);
        }

        private DashboardOrchestrator GetSut(ProviderDashboardSummary dashboardProjection)
        {
            var timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock.Setup(t => t.Today).Returns(_today);

            var serviceParameters = new ServiceParameters();

            var vacancyClientMock = new Mock<IProviderVacancyClient>();
            vacancyClientMock.Setup(c => c.GetDashboardSummary(Ukprn))
                .ReturnsAsync(dashboardProjection);

            _permissionServiceMock = new Mock<IProviderRelationshipsService>();
            
            _permissionServiceMock.Setup(p => p.CheckProviderHasPermissions(Ukprn, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(true);

            var userDetails = new User();

            _clientMock = new Mock<IRecruitVacancyClient>();
            
            _clientMock.Setup(c => c.GetUsersDetailsByDfEUserId(It.IsAny<string>()))
                .ReturnsAsync(userDetails);

            var alertsViewModel = new AlertsViewModel(null, null, Ukprn);
            var alertsFactoryMock = new Mock<IProviderAlertsViewModelFactory>();
            alertsFactoryMock.Setup(a => a.Create(userDetails))
                .ReturnsAsync(alertsViewModel);

            var orch = new DashboardOrchestrator(vacancyClientMock.Object, _clientMock.Object, alertsFactoryMock.Object, _permissionServiceMock.Object, serviceParameters);

            return orch;
        }
    }
}