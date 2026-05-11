namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.VacancyAnalytics;

public record GetVacancyAnalyticsByVacancyReferenceApiRequest(long VacancyReference) : IGetApiRequest
{
    public string GetUrl => $"vacancies/{VacancyReference}/analytics";
}