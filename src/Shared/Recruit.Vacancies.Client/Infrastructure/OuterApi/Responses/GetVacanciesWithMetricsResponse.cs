using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetVacanciesWithMetricsResponse
{
    [JsonProperty("vacancies")]
    public List<long> Vacancies { get; set; }
}