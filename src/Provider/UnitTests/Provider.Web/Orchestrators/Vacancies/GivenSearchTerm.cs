using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Vacancies;

public class GivenSearchTerm
{
    private readonly VacancyUser _user = new VacancyUser
    {
        Ukprn = 12345,
        UserId = "USER1",
    };
    private readonly VacancySummary[] _testVacancies =  
    {
        new() { Title="The quick brown", LegalEntityName="20th Century Fox", VacancyReference=1000000101, Status = VacancyStatus.Closed },
        new() { Title="fox jumped over", LegalEntityName="20th Century Fox", VacancyReference=1000000102, Status = VacancyStatus.Live },
        new() { Title="the lazy dog", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000103, Status = VacancyStatus.Live },
        new() { Title="The quick brown fox", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000104, Status = VacancyStatus.Live },
        new() { Title="the lazy fox", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000105, Status = VacancyStatus.Closed },
        new() { Title="brown dog", LegalEntityName="Black & Brown Ltd", VacancyReference=2000000105, Status = VacancyStatus.Referred }
    };

    [Test]
    public async Task ThenReturnResults()
    {
        var sut = GetSut(1, FilteringOptions.Live, "something"); 
            
        var result = await sut.GetVacanciesViewModelAsync(_user, "Live", 1, "something");
        result.Vacancies.Any().Should().BeTrue();
    }

    [TestCase("Submitted", "fox")]
    [TestCase("Live","xyz")]
    [TestCase("Submitted","")]
    public async Task ThenReturnNoResults(string status, string searchTerm)
    {
        Enum.TryParse<FilteringOptions>(status, out var vacancyStatus);
            
        var sut = GetSut(1, vacancyStatus, searchTerm + "1"); 
        var result = await sut.GetVacanciesViewModelAsync(_user, status, 1, "");
        result.Vacancies.Any().Should().BeFalse();
    }

    private VacanciesOrchestrator GetSut(int page, FilteringOptions? status, string searchTerm)
    {
        var providerClientMock = new Mock<IProviderVacancyClient>();
            
        providerClientMock.Setup(c => c.GetDashboardAsync(_user.Ukprn.Value, _user.UserId, page, 25, "CreatedDate", "Desc", status, searchTerm))
            .ReturnsAsync(new ProviderDashboard {
                Vacancies = _testVacancies
            });
        providerClientMock.Setup(c => c.GetDashboardAsync(_user.Ukprn.Value, _user.UserId, page, 25, "CreatedDate", "Desc", status, ""))
            .ReturnsAsync(new ProviderDashboard
            {
                Vacancies = new List<VacancySummary>(),
                TotalVacancies = 0
            });
        return new VacanciesOrchestrator(
            providerClientMock.Object,
            Mock.Of<IProviderRelationshipsService>(),
            Mock.Of<ITrainingProviderService>(),
            Mock.Of<IOuterApiClient>());
    }
}