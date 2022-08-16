using System;
using System.Collections.Generic;
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
        private Mock<IProviderRelationshipsService> _permissionServiceMock;

        [Theory]
        [InlineData(VacancyType.Apprenticeship)]
        [InlineData(VacancyType.Traineeship)]
        public async Task WhenHasVacancies_ShouldReturnViewModelAsync(VacancyType vacancyType)
        {
            var fixture = new Fixture();
            var dashboardProjection = fixture.Create<ProviderDashboardSummary>();

            var orch = GetSut(dashboardProjection, vacancyType);

            var actualDashboard = await orch.GetDashboardViewModelAsync(_user);

            actualDashboard.HasAnyVacancies.Should().BeTrue();
            actualDashboard.Alerts.Should().NotBeNull();
            actualDashboard.Ukprn.Should().Be(Ukprn);
        }

        [Theory]
        [InlineData(VacancyType.Apprenticeship)]
        [InlineData(VacancyType.Traineeship)]
        public async Task WhenHasNoVacancies_ShouldReturnViewModelAsync(VacancyType vacancyType)
        {
            var orch = GetSut(new ProviderDashboardSummary(), vacancyType);

            var actualDashboard = await orch.GetDashboardViewModelAsync(_user);

            actualDashboard.HasAnyVacancies.Should().BeFalse();
            actualDashboard.NoOfVacanciesClosingSoonWithNoApplications.Should().Be(0);
            actualDashboard.NoOfVacanciesClosingSoon.Should().Be(0);
            actualDashboard.Alerts.Should().NotBeNull();
            actualDashboard.Ukprn.Should().Be(Ukprn);
        }

        [Theory]
        [InlineData(VacancyType.Apprenticeship)]
        [InlineData(VacancyType.Traineeship)]
        public async Task Then_Checks_For_CorrectPermission_BasedOn_Vacancy_Type(VacancyType vacancyType)
        {
            var orch = GetSut(new ProviderDashboardSummary(), vacancyType);

            var actual = await orch.GetDashboardViewModelAsync(_user);

            if (vacancyType == VacancyType.Apprenticeship)
            {
                _permissionServiceMock.Verify(x=>x.GetLegalEntitiesForProviderAsync(Ukprn, OperationType.RecruitmentRequiresReview));
                actual.HasEmployerReviewPermission.Should().BeTrue();
            }
            if (vacancyType == VacancyType.Traineeship)
            {
                actual.HasEmployerReviewPermission.Should().BeFalse();
            }
            
        
        }

        private DashboardOrchestrator GetSut(ProviderDashboardSummary dashboardProjection, VacancyType vacancyType)
        {
            var timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock.Setup(t => t.Today).Returns(_today);

            var serviceParameters = new ServiceParameters(vacancyType.ToString());

            var fixture = new Fixture();

            var vacancyClientMock = new Mock<IProviderVacancyClient>();
            vacancyClientMock.Setup(c => c.GetDashboardSummary(Ukprn, vacancyType))
                .ReturnsAsync(dashboardProjection);

            _permissionServiceMock = new Mock<IProviderRelationshipsService>();
            
            _permissionServiceMock.Setup(p => p.GetLegalEntitiesForProviderAsync(Ukprn, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(new List<EmployerInfo>{new EmployerInfo()});

            var userDetails = new User();

            var clientMock = new Mock<IRecruitVacancyClient>();
            clientMock.Setup(c => c.GetUsersDetailsAsync(UserId))
                .ReturnsAsync(userDetails);

            var alertsViewModel = new AlertsViewModel(null, null, Ukprn);
            var alertsFactoryMock = new Mock<IProviderAlertsViewModelFactory>();
            alertsFactoryMock.Setup(a => a.Create(dashboardProjection, userDetails))
                .Returns(alertsViewModel);

            var orch = new DashboardOrchestrator(vacancyClientMock.Object, clientMock.Object, alertsFactoryMock.Object, _permissionServiceMock.Object, serviceParameters);

            return orch;
        }
    }
}