using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
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

        private readonly VacancyUser _user = new VacancyUser { UserId = UserId };
        private Mock<IRecruitVacancyClient> _clientMock;
        private Mock<IEmployerVacancyClient> _vacancyClientMock;

        [Fact]
        public async Task WhenHasVacancies_ShouldReturnViewModelAsync()
        {
            var dashboardOrchestrator = GetSut(new EmployerDashboardSummary
            {
                Live = 6,
                NumberClosingSoonWithNoApplications = 2,
                NumberClosingSoon = 4,
                NumberOfNewApplications = 3,
                NumberOfSharedApplications = 2
            });

            var actualDashboard = await dashboardOrchestrator.GetDashboardViewModelAsync(EmployerAccountId, _user);

            actualDashboard.EmployerAccountId.Should().Be(EmployerAccountId);
            actualDashboard.HasAnyVacancies.Should().BeTrue();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(2);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(4);
            actualDashboard.NoOfNewApplications.Should().Be(3);
            actualDashboard.NoOfSharedApplications.Should().Be(2);
            actualDashboard.Alerts.Should().NotBeNull();
            actualDashboard.HasEmployerReviewPermission.Should().BeTrue();
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

        [Fact]
        public async Task WhenUserNotExists_ThenCreated()
        {
            var dashboardOrchestrator = GetSut(new EmployerDashboardSummary());
            
            await dashboardOrchestrator.GetDashboardViewModelAsync(EmployerAccountId, new VacancyUser{UserId = "unknown", Email = "unknown"});
            
            _clientMock.Verify(x=>x.UserSignedInAsync(It.Is<VacancyUser>(c=>
                c.Name.Equals("First Last")
                && c.Email.Equals("unknown")
                && c.UserId.Equals("unknown")
                ), UserType.Employer), Times.Once);
        }
        
        [Fact]
        public async Task WhenNoVacancies_ThenSetupEmployerEventCreated()
        {
            var dashboardOrchestrator = GetSut(new EmployerDashboardSummary());
            
            await dashboardOrchestrator.GetDashboardViewModelAsync(EmployerAccountId, new VacancyUser{UserId = "unknown", Email = "unknown"});
            
            _vacancyClientMock.Verify(x=>x.SetupEmployerAsync(EmployerAccountId), Times.Once);
        }

        private DashboardOrchestrator GetSut(EmployerDashboardSummary dashboardSummary)
        {
            
            _vacancyClientMock = new Mock<IEmployerVacancyClient>();
            _vacancyClientMock.Setup(c => c.GetDashboardSummary(EmployerAccountId))
                .ReturnsAsync(dashboardSummary);

            var permissionServiceMock = new Mock<IProviderRelationshipsService>();
            permissionServiceMock.Setup(p => p.CheckEmployerHasPermissions(EmployerAccountId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(true);

            var userDetails = new User();

            _clientMock = new Mock<IRecruitVacancyClient>();
            _clientMock.Setup(c => c.GetUsersDetailsAsync(UserId))
                .ReturnsAsync(userDetails);
            
            _clientMock.SetupSequence(c => c.GetUsersDetailsAsync("unknown"))
                .ReturnsAsync((User) null)
                .ReturnsAsync(userDetails);

            _clientMock.Setup(x => x.GetEmployerIdentifiersAsync("unknown", "unknown")).ReturnsAsync(new GetUserAccountsResponse
            {
                FirstName = "First",
                LastName = "Last"
            });

            var alertsViewModel = new AlertsViewModel(null, null, null, null);
            var alertsFactoryMock = new Mock<IEmployerAlertsViewModelFactory>();
            alertsFactoryMock.Setup(a => a.Create(EmployerAccountId, userDetails))
                .ReturnsAsync(alertsViewModel);

            var dashboardOrchestrator = new DashboardOrchestrator(_vacancyClientMock.Object, _clientMock.Object, alertsFactoryMock.Object, permissionServiceMock.Object);

            return dashboardOrchestrator;
        }
    }
}