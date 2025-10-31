using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Report;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services;
[TestFixture]
internal class ProviderReportServiceTests
{
    [Test, MoqAutoData]
    public async Task GetReportsForProviderAsync_Should_Return_As_Expected(
        long ukprn,
        GetProviderReportsApiResponse response,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] ProviderReportService providerReportService)
    {
        var expectedGetUrl = new GetProviderReportsApiRequest(ukprn);
        outerApiClient.Setup(x => x.Get<GetProviderReportsApiResponse>(
                It.Is<GetProviderReportsApiRequest>(r => r.GetUrl == expectedGetUrl.GetUrl)))
            .ReturnsAsync(response);

        var result = await providerReportService.GetReportsForProviderAsync(ukprn);

        result.Should().BeEquivalentTo(response);
    }

    [Test, MoqAutoData]
    public async Task GetReportAsync_Should_Return_As_Expected(
        Guid reportId,
        GetReportApiResponse response,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] ProviderReportService providerReportService)
    {
        var expectedGetUrl = new GetReportApiRequest(reportId);
        outerApiClient.Setup(x => x.Get<GetReportApiResponse>(
                It.Is<GetReportApiRequest>(r => r.GetUrl == expectedGetUrl.GetUrl)))
            .ReturnsAsync(response);

        var result = await providerReportService.GetReportAsync(reportId);

        result.Should().BeEquivalentTo(response);
    }

    [Test, MoqAutoData]
    public async Task GetReportDataAsync_Should_Return_As_Expected(
        Guid reportId,
        GetReportDataApiResponse response,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] ProviderReportService providerReportService)
    {
        var expectedGetUrl = new GetReportDataApiRequest(reportId);
        outerApiClient.Setup(x => x.Get<GetReportDataApiResponse>(
                It.Is<GetReportDataApiRequest>(r => r.GetUrl == expectedGetUrl.GetUrl)))
            .ReturnsAsync(response);

        var result = await providerReportService.GetReportDataAsync(reportId);

        result.Should().BeEquivalentTo(response);
    }
}
