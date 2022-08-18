using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Vacancies
{
    public class GetVacanciesViewModelAsyncTests
    {
        const string EmployerAccountId = "ABCDE";

        [Fact]
        public async Task WhenHaveOver25Vacancies_ShouldShowPager()
        {
            var vacancies = new List<VacancySummary>();
            int totalVacancies = 27;
            for (var i = 26; i <= totalVacancies; i++)
            {
                vacancies.Add(new VacancySummary
                {
                    Title = i.ToString(),
                    Status = VacancyStatus.Submitted
                });
            }

            var orch = GetOrchestrator(vacancies, 2, FilteringOptions.Submitted, string.Empty, 27);

            var vm = await orch.GetVacanciesViewModelAsync(EmployerAccountId, "Submitted", 2, new VacancyUser(), string.Empty);

            vm.ShowResultsTable.Should().BeTrue();
            
            vm.Pager.ShowPager.Should().BeTrue();

            vm.Vacancies.Count.Should().Be(2);
            vm.Vacancies[0].Title.Should().Be("26");
            vm.Vacancies[1].Title.Should().Be("27");
        }

        [Fact]
        public async Task WhenHave25OrUnderVacancies_ShouldNotShowPager()
        {
            var vacancies = new List<VacancySummary>();
            for (var i = 1; i <= 25; i++)
            {
                vacancies.Add(new VacancySummary {
                    Title = i.ToString(),
                    Status = VacancyStatus.Submitted
                });
            }

            var orch = GetOrchestrator(vacancies, 2, FilteringOptions.Submitted, string.Empty, 25);

            var vm = await orch.GetVacanciesViewModelAsync(EmployerAccountId, "Submitted", 2, new VacancyUser(), string.Empty);

            vm.ShowResultsTable.Should().BeTrue();
            
            vm.Pager.ShowPager.Should().BeFalse();

            vm.Vacancies.Count.Should().Be(25);
        }


        private VacanciesOrchestrator GetOrchestrator(List<VacancySummary> vacancies, int page, FilteringOptions status, string searchTerm, int totalVacancies )
        {
            var timeProviderMock = new Mock<ITimeProvider>();

            var clientMock = new Mock<IEmployerVacancyClient>();
            clientMock.Setup(c => c.GetDashboardAsync(EmployerAccountId,page, status, searchTerm))
                .Returns(Task.FromResult(new EmployerDashboard
                {
                    Vacancies = vacancies
                }));
            
            var recruitClientMock = new Mock<IRecruitVacancyClient>();
            recruitClientMock.Setup(c => c.GetUsersDetailsAsync(It.IsAny<string>())).ReturnsAsync(new User());
            clientMock.Setup(x => x.GetVacancyCount(EmployerAccountId, VacancyType.Apprenticeship, FilteringOptions.Submitted, string.Empty))
                .ReturnsAsync(totalVacancies);
            
            var alertsFactoryMock = new Mock<IEmployerAlertsViewModelFactory>();

            return new VacanciesOrchestrator(clientMock.Object, timeProviderMock.Object, recruitClientMock.Object, alertsFactoryMock.Object);
        }
    }
}
