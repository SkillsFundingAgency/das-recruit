using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Dashboard
{
    public class GetDashboardViewModelAsyncTests
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

            var vm = await orch.GetDashboardViewModelAsync(EmployerAccountId, "Submitted", 2, new VacancyUser());

            vm.ShowResultsTable.Should().BeTrue();
            vm.HasVacancies.Should().BeTrue();
            
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

            var vm = await orch.GetDashboardViewModelAsync(EmployerAccountId, "Submitted", 2, new VacancyUser());

            vm.ShowResultsTable.Should().BeTrue();
            vm.HasVacancies.Should().BeTrue();

            vm.Pager.ShowPager.Should().BeFalse();

            vm.Vacancies.Count.Should().Be(25);
        }

        private DashboardOrchestrator GetOrchestrator(List<VacancySummary> vacancies)
        {
            var timeProviderMock = new Mock<ITimeProvider>();

            var clientMock = new Mock<IEmployerVacancyClient>();
            clientMock.Setup(c => c.GetDashboardAsync(EmployerAccountId))
                .Returns(Task.FromResult(new EmployerDashboard
                {
                    Vacancies = vacancies
                }));
            
            var recruitClientMock = new Mock<IRecruitVacancyClient>();
            recruitClientMock.Setup(c => c.GetUsersDetailsAsync(It.IsAny<string>())).ReturnsAsync(new User());

            return new DashboardOrchestrator(clientMock.Object, timeProviderMock.Object, recruitClientMock.Object);
        }
    }
}
