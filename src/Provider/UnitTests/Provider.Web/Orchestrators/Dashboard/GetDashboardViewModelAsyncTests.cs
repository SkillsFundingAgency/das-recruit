using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Employer.Web.Orchestrators.Dashboard
{
    public class GetDashboardViewModelAsyncTests
    {
        private const long Ukprn = 12345678;
        private const string UserId = "user id";

        private readonly DateTime _today = DateTime.Parse("2019-09-18");
        private readonly VacancyUser _user = new VacancyUser {UserId = UserId,  Ukprn = Ukprn };

        [Fact]
        public async Task WhenHasVacancies_ShouldReturnViewModelAsync()
        {
            var vacancies = new List<VacancySummary>()
                {
                    new VacancySummary{ClosingDate = _today.AddDays(4), Status = VacancyStatus.Live, ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship, NoOfNewApplications = 0}, //Should be included in NoOfVacanciesClosingSoonWithNoApplications & NoOfVacanciesClosingSoon
                    new VacancySummary{ClosingDate = _today.AddDays(5), Status = VacancyStatus.Live, ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship, NoOfNewApplications = 0}, //Should be included in NoOfVacanciesClosingSoonWithNoApplications & NoOfVacanciesClosingSoon
                    new VacancySummary{ClosingDate = _today.AddDays(6), Status = VacancyStatus.Live, ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship, NoOfNewApplications = 0}, //Should NOT be included in NoOfVacanciesClosingSoonWithNoApplications OR NoOfVacanciesClosingSoon
                    new VacancySummary{ClosingDate = _today.AddDays(5), Status = VacancyStatus.Live, ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship, NoOfNewApplications = 100000}, //Should NOT be included in NoOfVacanciesClosingSoonWithNoApplications. Should be included in NoOfVacanciesClosingSoon
                    new VacancySummary{ClosingDate = _today.AddDays(5), Status = VacancyStatus.Live}, //Should be included in NoOfVacanciesClosingSoon
                    new VacancySummary{ClosingDate = _today.AddDays(6), Status = VacancyStatus.Live}, //Should NOT be included in NoOfVacanciesClosingSoon
                };

            var orch = GetSut(vacancies);

            var actualDashboard = await orch.GetDashboardViewModelAsync(_user);

            actualDashboard.Vacancies.Should().Equal(vacancies);
            actualDashboard.HasAnyVacancies.Should().BeTrue();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(2);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(4);
            actualDashboard.Alerts.Should().NotBeNull();
            actualDashboard.Ukprn.Should().Be(Ukprn);
        }

        [Fact]
        public async Task WhenHasNoVacancies_ShouldReturnViewModelAsync()
        {
            var vacancies = new List<VacancySummary>();
                
            var orch = GetSut(vacancies);

            var actualDashboard = await orch.GetDashboardViewModelAsync(_user);

            actualDashboard.Vacancies.Should().Equal(vacancies);
            actualDashboard.HasAnyVacancies.Should().BeFalse();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(0);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(0);
            actualDashboard.Alerts.Should().NotBeNull();
            actualDashboard.Ukprn.Should().Be(Ukprn);
        }

        private DashboardOrchestrator GetSut(List<VacancySummary> vacancies)
        {
            var timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock.Setup(t => t.Today).Returns(_today);

            var dashboardProjection = new ProviderDashboard
            {
                Vacancies = vacancies
            };

            var vacancyClientMock = new Mock<IProviderVacancyClient>();
            vacancyClientMock.Setup(c => c.GetDashboardAsync(Ukprn, true))
                .ReturnsAsync(dashboardProjection);

            var permissionServiceMock = new Mock<IProviderRelationshipsService>();
            permissionServiceMock.Setup(p => p.GetLegalEntitiesForProviderAsync(Ukprn, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(new List<EmployerInfo>());

            var userDetails = new User();

            var clientMock = new Mock<IRecruitVacancyClient>();
            clientMock.Setup(c => c.GetUsersDetailsAsync(UserId))
                .ReturnsAsync(userDetails);

            var alertsViewModel = new AlertsViewModel(null, null, Ukprn);
            var alertsFactoryMock = new Mock<IProviderAlertsViewModelFactory>();
            alertsFactoryMock.Setup(a => a.Create(dashboardProjection, userDetails))
                .Returns(alertsViewModel);

            var orch = new DashboardOrchestrator(vacancyClientMock.Object, timeProviderMock.Object, clientMock.Object, alertsFactoryMock.Object, permissionServiceMock.Object);

            return orch;
        }
    }
}