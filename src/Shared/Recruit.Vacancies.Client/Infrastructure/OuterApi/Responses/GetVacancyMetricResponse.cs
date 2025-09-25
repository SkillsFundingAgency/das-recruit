using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetVacancyMetricResponse
{
    [JsonProperty("vacancyReference")]
    public string VacancyReference { get; set; }
    [JsonProperty("viewsCount")]
    public int ViewsCount { get; set; }

    [JsonProperty("searchResultsCount")]
    public int SearchResultsCount { get; set; }

    [JsonProperty("applicationStartedCount")]
    public int ApplicationStartedCount { get; set; }

    [JsonProperty("applicationSubmittedCount")]
    public int ApplicationSubmittedCount { get; set; }
}