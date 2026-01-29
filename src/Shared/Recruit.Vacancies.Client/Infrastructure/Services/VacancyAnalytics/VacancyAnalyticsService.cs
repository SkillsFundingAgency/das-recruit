using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.VacancyAnalytics;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancyAnalytics;

public class VacancyAnalyticsService(
    ILogger<VacancyAnalyticsService> logger,
    IOuterApiClient outerApiClient) : IVacancyAnalyticsService
{
    public async Task<GetVacancyAnalyticsByVacancyReferenceApiResponse> GetVacancyAnalyticsSummaryAsync(long vacancyReference)
    {
        logger.LogTrace("Getting vacancy analytics details from Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        var result = await retryPolicy.Execute(_ =>
                outerApiClient.Get<GetVacancyAnalyticsByVacancyReferenceApiResponse>(new GetVacancyAnalyticsByVacancyReferenceApiRequest(vacancyReference)),
            new Dictionary<string, object>
            {
                {
                    "apiCall", "Vacancy"
                }
            });

        return result;
    }
}