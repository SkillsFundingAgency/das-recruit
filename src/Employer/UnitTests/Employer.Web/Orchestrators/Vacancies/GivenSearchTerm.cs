using System.Linq;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Vacancies;

public class GivenSearchTerm
{
    private const string EmployerAccountId = "XXXXXX";

    private readonly VacancyUser _user;
    private readonly Mock<IRecruitVacancyClient> _recruitVacancyClientMock;
    private readonly Mock<IEmployerAlertsViewModelFactory> _employerAlertsViewModelFactoryMock;

    private readonly VacancySummary[] _testVacancies = {
        new VacancySummary{Title="The quick brown", LegalEntityName="20th Century Fox", VacancyReference=1000000101, Status = VacancyStatus.Closed},
        new VacancySummary{Title="fox jumped over", LegalEntityName="20th Century Fox", VacancyReference=1000000102, Status = VacancyStatus.Live},
        new VacancySummary{Title="the lazy dog", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000103, Status = VacancyStatus.Live},
        new VacancySummary{Title="The quick brown fox", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000104, Status = VacancyStatus.Live},
        new VacancySummary{Title="the lazy fox", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000105, Status = VacancyStatus.Closed},
        new VacancySummary{Title="brown dog", LegalEntityName="Black & Brown Ltd", VacancyReference=2000000105, Status = VacancyStatus.Referred}
    };

    [TestCase("Closed", "lazy")]
    public async Task ThenReturnResults(string status, string searchTerm)
    {
        Enum.TryParse<FilteringOptions>(status, out var vacancyStatus);
        var sut = GetSut(searchTerm, 1, vacancyStatus, 6, _testVacancies); 
        var result = await sut.GetVacanciesViewModelAsync(EmployerAccountId, status, 1, _user, searchTerm);
        result.Vacancies.Any().Should().BeTrue();
        result.Vacancies.Count.Should().Be(6);
            
    }

    [Test]
    public async Task ThenReturnNoResults()
    {
        Enum.TryParse<FilteringOptions>("Live", out var vacancyStatus);
        var sut = GetSut("fox", 1, vacancyStatus, 0, []); 
        var result = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, _user, "fox");
        result.Vacancies.Any().Should().BeFalse();
    }

    private VacanciesOrchestrator GetSut(string searchTerm, int page, FilteringOptions? status, int vacanciesCount, VacancySummary[] vacancySummaries)
    {
        var employerClientMock = new Mock<IEmployerVacancyClient>();
        employerClientMock.Setup(c => c.GetDashboardAsync(EmployerAccountId, _user.UserId, page, 25, "CreatedDate", "Desc", status, searchTerm))
            .ReturnsAsync(new EmployerDashboard {
                Vacancies = vacancySummaries,
                TotalVacancies = vacanciesCount

            });
        return new VacanciesOrchestrator(employerClientMock.Object, Mock.Of<IEmployerAccountProvider>(), Mock.Of<IOuterApiClient>(), Mock.Of<IEncodingService>());
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
        var userDetails = new User
        {
            Email = _user.Email,
            Name = _user.Name,
            Ukprn = _user.Ukprn,
            Id = userId
        };

        _recruitVacancyClientMock = new Mock<IRecruitVacancyClient>();
        _recruitVacancyClientMock
            .Setup(x => x.GetUsersDetailsAsync(_user.UserId))
            .ReturnsAsync(userDetails);

        _employerAlertsViewModelFactoryMock = new Mock<IEmployerAlertsViewModelFactory>();
    }
}