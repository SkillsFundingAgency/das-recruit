using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Reports;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Report;
public class ProviderReportService(
    ILogger<ProviderReportService> logger,
    IOuterApiClient outerApiClient) : IProviderReportService
{
    public async Task<GetProviderReportsApiResponse> GetReportsForProviderAsync(long ukprn)
    {
        logger.LogTrace("Getting Provider reports from Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        var result = await retryPolicy.Execute(_ =>
                outerApiClient.Get<GetProviderReportsApiResponse>(new GetProviderReportsApiRequest(ukprn)),
            new Dictionary<string, object>
            {
                {
                    "apiCall", "Reports"
                }
            });

        return result;
    }

    public async Task CreateProviderApplicationsReportAsync(Guid reportId, long ukprn, DateTime fromDate, DateTime toDate,
        VacancyUser user,
        string reportName)
    {
        logger.LogTrace("Posting Reports to Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        await retryPolicy.Execute(_ =>
                outerApiClient.Post(new PostCreateReportApiRequest(new PostCreateReportApiRequest.PostCreateReportApiRequestData
                {
                    Id = reportId,
                    ToDate = toDate,
                    FromDate = fromDate,
                    OwnerType = nameof(ReportOwnerType.Provider),
                    CreatedBy = user.Name,
                    Ukprn = ukprn,
                    UserId = user.UserId,
                    Name = reportName
                })),
            new Dictionary<string, object>
            {
                {
                    "apiCall", "Reports"
                }
            });
    }

    public async Task<GetReportApiResponse> GetReportAsync(Guid reportId)
    {
        logger.LogTrace("Getting Report Details from Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();
        var result = await retryPolicy.Execute(_ =>
                outerApiClient.Get<GetReportApiResponse>(new GetReportApiRequest(reportId)),
            new Dictionary<string, object>
            {
                {
                    "apiCall", "Reports"
                }
            });
        return result;
    }

    public async Task<GetReportDataApiResponse> GetReportDataAsync(Guid reportId)
    {
        logger.LogTrace("Getting Report Data from Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();
        var result = await retryPolicy.Execute(_ =>
                outerApiClient.Get<GetReportDataApiResponse>(new GetReportDataApiRequest(reportId)),
            new Dictionary<string, object>
            {
                {
                    "apiCall", "Reports"
                }
            });
        return result;
    }
}