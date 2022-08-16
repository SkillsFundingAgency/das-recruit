using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.Vacancies
{
    public class GivenSearchTerm
    {
        private readonly VacancyUser _user = new VacancyUser
        {
            Ukprn = 12345
        };
        private VacancySummary[] _testVacancies =  
        {
            new VacancySummary(){Title="The quick brown", LegalEntityName="20th Century Fox", VacancyReference=1000000101, Status = VacancyStatus.Closed},
            new VacancySummary(){Title="fox jumped over", LegalEntityName="20th Century Fox", VacancyReference=1000000102, Status = VacancyStatus.Live},
            new VacancySummary(){Title="the lazy dog", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000103, Status = VacancyStatus.Live},
            new VacancySummary(){Title="The quick brown fox", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000104, Status = VacancyStatus.Live},
            new VacancySummary(){Title="the lazy fox", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000105, Status = VacancyStatus.Closed},
            new VacancySummary(){Title="brown dog", LegalEntityName="Black & Brown Ltd", VacancyReference=2000000105, Status = VacancyStatus.Referred}
        };

        [Fact]
        public async Task ThenReturnResults()
        {
            var sut = GetSut(1, FilteringOptions.Live, "something"); 
            
            var result = await sut.GetVacanciesViewModelAsync(_user, "Live", 1, "something");
            result.Vacancies.Any().Should().BeTrue();
        }

        [Theory]
        [InlineData("Submitted", "fox")]
        [InlineData("Live","xyz")]
        [InlineData("Submitted","")]
        public async Task ThenReturnNoResults(string status, string searchTerm)
        {
            Enum.TryParse<FilteringOptions>(status, out var vacancyStatus);
            
            var sut = GetSut(1, vacancyStatus, searchTerm + "1"); 
            var result = await sut.GetVacanciesViewModelAsync(_user, status, 1, searchTerm);
            result.Vacancies.Any().Should().BeFalse();
        }

        private VacanciesOrchestrator GetSut(int page, FilteringOptions? status, string searchTerm)
        {
            var providerClientMock = new Mock<IProviderVacancyClient>();
            
            providerClientMock.Setup(c => c.GetDashboardAsync(_user.Ukprn.Value, VacancyType.Apprenticeship,page, status, searchTerm))
                .ReturnsAsync(new ProviderDashboard {
                    Vacancies = _testVacancies
                });
            return new VacanciesOrchestrator(
                providerClientMock.Object,
                Mock.Of<IRecruitVacancyClient>(),
               Mock.Of<IProviderAlertsViewModelFactory>(),
                Mock.Of<IProviderRelationshipsService>(), new ServiceParameters(VacancyType.Apprenticeship.ToString()));
        }

    }
}