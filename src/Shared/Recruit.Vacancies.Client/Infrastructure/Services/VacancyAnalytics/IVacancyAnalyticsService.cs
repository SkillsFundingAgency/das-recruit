using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.VacancyAnalytics;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancyAnalytics;

public interface IVacancyAnalyticsService
{
    Task<GetVacancyAnalyticsByVacancyReferenceApiResponse> GetVacancyAnalyticsSummaryAsync(long vacancyReference);
}