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
            for (var i = 1; i <= 27; i++)
            {
                vacancies.Add(new VacancySummary
                {
                    Title = i.ToString(),
                    Status = VacancyStatus.Submitted
                });
            }

            var orch = GetOrchestrator(vacancies);

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

            var orch = GetOrchestrator(vacancies);

            var vm = await orch.GetVacanciesViewModelAsync(EmployerAccountId, "Submitted", 2, new VacancyUser(), string.Empty);

            vm.ShowResultsTable.Should().BeTrue();
            
            vm.Pager.ShowPager.Should().BeFalse();

            vm.Vacancies.Count.Should().Be(25);
        }

        [Fact]
        public async Task SearchFilterResults_ShouldReturnSingleVacancy()
        {
            var searchTerm = "VacancyTitle_22";
            var vacancies = new List<VacancySummary>();
            for (var i = 1; i <= 25; i++)
            {
                vacancies.Add(new VacancySummary
                {
                    Title = "VacancyTitle_" + i,
                    Status = VacancyStatus.Submitted
                });
            }

            var orch = GetOrchestrator(vacancies);
            var vm = await orch.GetVacanciesViewModelAsync(EmployerAccountId, "Submitted", 2, new VacancyUser(), searchTerm);
            vm.ShowResultsTable.Should().BeTrue();
            vm.Vacancies.Count.Should().Be(1);
            vm.Vacancies.FirstOrDefault().Title.Should().BeEquivalentTo(searchTerm);
        }

        [Fact] 
        public async Task SearchFilterResults_ShouldReturnAllMatchingVacancies()
        {
            var searchTerm = "VacancyTitle_";
            var vacancies = new List<VacancySummary>();
            for (var i = 1; i <= 25; i++)
            {
                vacancies.Add(new VacancySummary
                {
                    Title = "VacancyTitle_" + i,
                    Status = VacancyStatus.Submitted
                });
            }

            var orch = GetOrchestrator(vacancies);
            var vm = await orch.GetVacanciesViewModelAsync(EmployerAccountId, "Submitted", 2, new VacancyUser(), searchTerm);
            vm.ShowResultsTable.Should().BeTrue();
            vm.Vacancies.Count.Should().Be(vacancies.Count);
            vm.Vacancies.All(x => x.Title.Contains(searchTerm));
        }


        private VacanciesOrchestrator GetOrchestrator(List<VacancySummary> vacancies)
        {
            var timeProviderMock = new Mock<ITimeProvider>();

            var clientMock = new Mock<IEmployerVacancyClient>();
            clientMock.Setup(c => c.GetDashboardAsync(EmployerAccountId, true))
                .Returns(Task.FromResult(new EmployerDashboard
                {
                    Vacancies = vacancies
                }));
            
            var recruitClientMock = new Mock<IRecruitVacancyClient>();
            recruitClientMock.Setup(c => c.GetUsersDetailsAsync(It.IsAny<string>())).ReturnsAsync(new User());

            var alertsFactoryMock = new Mock<IEmployerAlertsViewModelFactory>();

            return new VacanciesOrchestrator(clientMock.Object, timeProviderMock.Object, recruitClientMock.Object, alertsFactoryMock.Object);
        }
    }
}
