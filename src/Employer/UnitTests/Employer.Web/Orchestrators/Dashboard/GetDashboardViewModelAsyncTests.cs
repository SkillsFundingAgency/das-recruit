using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Dashboard
{
    public class GetDashboardViewModelAsyncTests
    {
        [Fact]
        public async Task WhenHaveOver25Vacancies_ShouldShowPager()
        {
            const string employerAccountId = "ABCDE";

            var vacancies = new List<VacancySummary>();
            for (var i = 1; i <= 27; i++)
            {
                vacancies.Add(new VacancySummary
                {
                    Title = i.ToString(),
                    Status = VacancyStatus.Submitted
                });
            }

            var clientMock = new Mock<IEmployerVacancyClient>();
            var timeProviderMock = new Mock<ITimeProvider>();
            clientMock.Setup(c => c.GetDashboardAsync(employerAccountId))
                .Returns(Task.FromResult(new EmployerDashboard {
                    Vacancies = vacancies
                }));

            var orch = new DashboardOrchestrator(clientMock.Object, timeProviderMock.Object);

            var vm = await orch.GetDashboardViewModelAsync(employerAccountId, "Submitted", 2);

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
            const string employerAccountId = "ABCDE";

            var vacancies = new List<VacancySummary>();
            for (var i = 1; i <= 25; i++)
            {
                vacancies.Add(new VacancySummary {
                    Title = i.ToString(),
                    Status = VacancyStatus.Submitted
                });
            }

            var clientMock = new Mock<IEmployerVacancyClient>();
            var timeProviderMock = new Mock<ITimeProvider>();
            clientMock.Setup(c => c.GetDashboardAsync(employerAccountId))
                .Returns(Task.FromResult(new EmployerDashboard {
                    Vacancies = vacancies
                }));

            var orch = new DashboardOrchestrator(clientMock.Object, timeProviderMock.Object);

            var vm = await orch.GetDashboardViewModelAsync(employerAccountId, "Submitted", 2);

            vm.ShowResultsTable.Should().BeTrue();
            vm.HasVacancies.Should().BeTrue();

            vm.Pager.ShowPager.Should().BeFalse();

            vm.Vacancies.Count.Should().Be(25);
        }
    }
}
