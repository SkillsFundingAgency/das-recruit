using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.Vacancies
{
    public class GivenSearchTerm
    {
        private const string EmployerAccountId = "XXXXXX";

        private VacancyUser _user;
        private User _userDetails;
        private Mock<IRecruitVacancyClient> _recruitVacancyClientMock;
        private Mock<IEmployerAlertsViewModelFactory> _employerAlertsViewModelFactoryMock;

        private VacancySummary[] _testVacancies = new[] 
        {
            new VacancySummary(){Title="The quick brown", LegalEntityName="20th Century Fox", VacancyReference=1000000101, Status = VacancyStatus.Closed},
            new VacancySummary(){Title="fox jumped over", LegalEntityName="20th Century Fox", VacancyReference=1000000102, Status = VacancyStatus.Live},
            new VacancySummary(){Title="the lazy dog", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000103, Status = VacancyStatus.Live},
            new VacancySummary(){Title="The quick brown fox", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000104, Status = VacancyStatus.Live},
            new VacancySummary(){Title="the lazy fox", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000105, Status = VacancyStatus.Closed},
            new VacancySummary(){Title="brown dog", LegalEntityName="Black & Brown Ltd", VacancyReference=2000000105, Status = VacancyStatus.Referred}
        };

        [Theory]
        [InlineData("Closed", "lazy")]
        public async Task ThenReturnResults(string status, string searchTerm)
        {
            var sut = GetSut(searchTerm, 1, status); 
            var result = await sut.GetVacanciesViewModelAsync(EmployerAccountId, status, 1, _user, searchTerm);
            result.Vacancies.Any().Should().BeTrue();
            result.Vacancies.Count.Should().Be(6);
            
        }

        [Fact]
        public async Task ThenReturnNoResults()
        {
            var sut = GetSut("fox", 1, "Live"); 
            var result = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, _user, "test");
            result.Vacancies.Any().Should().BeFalse();
        }

        private VacanciesOrchestrator GetSut(string searchTerm, int page, string status)
        {
            Enum.TryParse<FilteringOptions>(status, out var filter);
            
            var employerClientMock = new Mock<IEmployerVacancyClient>();
            var timeProviderMock = new Mock<ITimeProvider>();
            employerClientMock.Setup(c => c.GetDashboardAsync(EmployerAccountId, page, filter, searchTerm))
                .ReturnsAsync(new EmployerDashboard {
                    Vacancies = _testVacancies
                });
            return new VacanciesOrchestrator(
                employerClientMock.Object,
                timeProviderMock.Object,
                _recruitVacancyClientMock.Object,
                _employerAlertsViewModelFactoryMock.Object);
        }

        public GivenSearchTerm()
        {
            var userId = Guid.NewGuid();
            _user = new VacancyUser
            {
                Email = "me@home.com",
                UserId = userId.ToString(),
                Name = "Bob Monkhouse",
                Ukprn = 12345678
            };
            _userDetails = new User
            {
                Email = _user.Email,
                Name = _user.Name,
                Ukprn = _user.Ukprn,
                Id = userId
            };

            _recruitVacancyClientMock = new Mock<IRecruitVacancyClient>();
            _recruitVacancyClientMock
                .Setup(x => x.GetUsersDetailsAsync(_user.UserId))
                .ReturnsAsync(_userDetails);

            _employerAlertsViewModelFactoryMock = new Mock<IEmployerAlertsViewModelFactory>();
        }

    }
}