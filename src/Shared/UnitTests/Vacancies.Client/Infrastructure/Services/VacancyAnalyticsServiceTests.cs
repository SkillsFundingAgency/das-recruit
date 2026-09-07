using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancyAnalytics;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services;

[TestFixture]
internal class VacancyAnalyticsServiceTests
{
    [Test, MoqAutoData]
    public async Task GetVacancyAnalyticsSummaryAsync_Should_Return_As_Expected(
        long vacancyReference,
        GetVacancyAnalyticsByVacancyReferenceApiResponse response,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] VacancyAnalyticsService service)
    {
        var expectedGetUrl = new GetVacancyAnalyticsByVacancyReferenceApiRequest(vacancyReference);
        outerApiClient.Setup(x => x.Get<GetVacancyAnalyticsByVacancyReferenceApiResponse>(
                It.Is<GetVacancyAnalyticsByVacancyReferenceApiRequest>(r => r.GetUrl == expectedGetUrl.GetUrl)))
            .ReturnsAsync(response);

        var result = await service.GetVacancyAnalyticsSummaryAsync(vacancyReference);

        result.Should().BeEquivalentTo(response);
    }
}