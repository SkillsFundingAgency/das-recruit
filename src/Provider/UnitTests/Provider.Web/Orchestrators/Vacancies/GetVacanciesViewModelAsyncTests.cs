using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Vacancies
{
    public class GetVacanciesViewModelAsyncTests
    {
        private VacancyUser _user;
        private User _userDetails;
        private Mock<IProviderAlertsViewModelFactory> _providerAlertsViewModelFactoryMock;
        private Mock<IRecruitVacancyClient> _recruitVacancyClientMock;

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

            var providerClientMock = new Mock<IProviderVacancyClient>();
            var timeProviderMock = new Mock<ITimeProvider>();
            providerClientMock.Setup(c => c.GetDashboardAsync(_user.Ukprn.Value, true))
                .Returns(Task.FromResult(new ProviderDashboard {
                    Vacancies = vacancies
                }));

            var orch = new VacanciesOrchestrator(
                providerClientMock.Object,
                _recruitVacancyClientMock.Object,
                timeProviderMock.Object,
                _providerAlertsViewModelFactoryMock.Object);

            var vm = await orch.GetVacanciesViewModelAsync(_user, "Submitted", 2, string.Empty);

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

            var providerClientMock = new Mock<IProviderVacancyClient>();
            var timeProviderMock = new Mock<ITimeProvider>();
            providerClientMock.Setup(c => c.GetDashboardAsync(_user.Ukprn.Value, true))
                .Returns(Task.FromResult(new ProviderDashboard {
                    Vacancies = vacancies
                }));

            var orch = new VacanciesOrchestrator(
                providerClientMock.Object,
                _recruitVacancyClientMock.Object,
                timeProviderMock.Object,
                _providerAlertsViewModelFactoryMock.Object);

            var vm = await orch.GetVacanciesViewModelAsync(_user, "Submitted", 2, string.Empty);

            vm.ShowResultsTable.Should().BeTrue();

            vm.Pager.ShowPager.Should().BeFalse();

            vm.Vacancies.Count.Should().Be(25);
        }

        public GetVacanciesViewModelAsyncTests()
        {
            var userId = Guid.NewGuid();
            _user = new VacancyUser
            {
                Email = "F.Sinatra@gmail.com",
                Name = "Frank Sinatra",
                Ukprn = 54321,
                UserId = userId.ToString()
            };
            _userDetails = new User
            {
                Ukprn = _user.Ukprn,
                Email = _user.Email,
                Name = _user.Name,
                Id = userId
            };

            _recruitVacancyClientMock = new Mock<IRecruitVacancyClient>();
            _recruitVacancyClientMock
                .Setup(x => x.GetUsersDetailsAsync(_user.UserId))
                .ReturnsAsync(_userDetails);

            _providerAlertsViewModelFactoryMock = new Mock<IProviderAlertsViewModelFactory>();
        }
    }
}
