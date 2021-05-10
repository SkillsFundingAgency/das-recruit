using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Dashboard
{
    public class GetDashboardViewModelAsyncTests
    {
        private const string employerAccountId = "XXXXXX";
        private const string UserId = "user id";

        private readonly DateTime _today = DateTime.Parse("2019-09-18");
        private readonly VacancyUser _user = new VacancyUser { UserId = UserId };

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

            var actualDashboard = await orch.GetDashboardViewModelAsync(employerAccountId, _user);

            actualDashboard.EmployerAccountId.Should().Be(employerAccountId);
            actualDashboard.Vacancies.Should().Equal(vacancies);
            actualDashboard.HasAnyVacancies.Should().BeTrue();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(2);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(4);
            actualDashboard.Alerts.Should().NotBeNull();
        }

        [Fact]
        public async Task WhenHasNoVacancies_ShouldReturnViewModelAsync()
        {
            var vacancies = new List<VacancySummary>();
                
            var orch = GetSut(vacancies);

            var actualDashboard = await orch.GetDashboardViewModelAsync(employerAccountId, _user);

            actualDashboard.EmployerAccountId.Should().Be(employerAccountId);
            actualDashboard.Vacancies.Should().Equal(vacancies);
            actualDashboard.HasAnyVacancies.Should().BeFalse();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(0);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(0);
            actualDashboard.Alerts.Should().NotBeNull();
        }

        private DashboardOrchestrator GetSut(List<VacancySummary> vacancies)
        {
            var timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock.Setup(t => t.Today).Returns(_today);

            var dashboardProjection = new EmployerDashboard
            {
                Vacancies = vacancies
            };

            var vacancyClientMock = new Mock<IEmployerVacancyClient>();
            vacancyClientMock.Setup(c => c.GetDashboardAsync(employerAccountId, true))
                .ReturnsAsync(dashboardProjection);

            var permissionServiceMock = new Mock<IProviderRelationshipsService>();
            permissionServiceMock.Setup(p => p.GetLegalEntitiesForProviderAsync(employerAccountId, "RecruitmentRequiresReview"))
                .ReturnsAsync(new List<EmployerInfo>());

            var userDetails = new User();

            var clientMock = new Mock<IRecruitVacancyClient>();
            clientMock.Setup(c => c.GetUsersDetailsAsync(UserId))
                .ReturnsAsync(userDetails);

            var alertsViewModel = new AlertsViewModel(null, null, null, null);
            var alertsFactoryMock = new Mock<IEmployerAlertsViewModelFactory>();
            alertsFactoryMock.Setup(a => a.Create(vacancies, userDetails))
                .Returns(alertsViewModel);

            var orch = new DashboardOrchestrator(vacancyClientMock.Object, timeProviderMock.Object, clientMock.Object, alertsFactoryMock.Object, permissionServiceMock.Object);

            return orch;
        }
    }
}