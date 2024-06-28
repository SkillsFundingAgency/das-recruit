namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetVacancyMetricResponse
{
    public int ViewsCount { get; set; }
    public int SearchResultsCount { get; set; }
    public int ApplicationStartedCount { get; set; }
    public int ApplicationSubmittedCount { get; set; }
}