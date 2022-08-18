using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
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
        private const string EmployerAccountId = "XXXXXX";
        private const string UserId = "user id";

        private readonly DateTime _today = DateTime.Parse("2019-09-18");
        private readonly VacancyUser _user = new VacancyUser { UserId = UserId };

        [Fact]
        public async Task WhenHasVacancies_ShouldReturnViewModelAsync()
        {
            var dashboardOrchestrator = GetSut(new EmployerDashboardSummary
            {
                Live = 6,
                NumberClosingSoonWithNoApplications = 2,
                NumberClosingSoon = 4,
                NumberOfNewApplications = 3
            });

            var actualDashboard = await dashboardOrchestrator.GetDashboardViewModelAsync(EmployerAccountId, _user);

            actualDashboard.EmployerAccountId.Should().Be(EmployerAccountId);
            actualDashboard.HasAnyVacancies.Should().BeTrue();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(2);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(4);
            actualDashboard.NoOfNewApplications.Should().Be(3);
            actualDashboard.Alerts.Should().NotBeNull();
        }

        [Fact]
        public async Task WhenHasNoVacancies_ShouldReturnViewModelAsync()
        {
            var dashboardOrchestrator = GetSut(new EmployerDashboardSummary());

            var actualDashboard = await dashboardOrchestrator.GetDashboardViewModelAsync(EmployerAccountId, _user);

            actualDashboard.EmployerAccountId.Should().Be(EmployerAccountId);
            actualDashboard.HasAnyVacancies.Should().BeFalse();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(0);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(0);
            actualDashboard.Alerts.Should().NotBeNull();
        }

        private DashboardOrchestrator GetSut(EmployerDashboardSummary dashboardSummary)
        {
            var timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock.Setup(t => t.Today).Returns(_today);

            var vacancyClientMock = new Mock<IEmployerVacancyClient>();
            vacancyClientMock.Setup(c => c.GetDashboardSummary(EmployerAccountId))
                .ReturnsAsync(dashboardSummary);

            var permissionServiceMock = new Mock<IProviderRelationshipsService>();
            permissionServiceMock.Setup(p => p.GetLegalEntitiesForProviderAsync(EmployerAccountId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(new List<EmployerInfo>());

            var userDetails = new User();

            var clientMock = new Mock<IRecruitVacancyClient>();
            clientMock.Setup(c => c.GetUsersDetailsAsync(UserId))
                .ReturnsAsync(userDetails);

            var alertsViewModel = new AlertsViewModel(null, null, null, null);
            var alertsFactoryMock = new Mock<IEmployerAlertsViewModelFactory>();
            alertsFactoryMock.Setup(a => a.Create(EmployerAccountId, userDetails))
                .ReturnsAsync(alertsViewModel);

            var dashboardOrchestrator = new DashboardOrchestrator(vacancyClientMock.Object, timeProviderMock.Object, clientMock.Object, alertsFactoryMock.Object, permissionServiceMock.Object);

            return dashboardOrchestrator;
        }
    }
}