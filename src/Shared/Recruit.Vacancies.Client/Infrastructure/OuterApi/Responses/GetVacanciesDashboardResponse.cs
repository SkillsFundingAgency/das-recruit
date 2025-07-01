using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetVacanciesDashboardResponse
{
    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }

    [JsonProperty("pageIndex")]
    public int PageIndex { get; set; }

    [JsonProperty("pageSize")]
    public int PageSize { get; set; }

    [JsonProperty("totalPages")]
    public int TotalPages { get; set; }

    [JsonProperty("hasPreviousPage")]
    public bool HasPreviousPage { get; set; }

    [JsonProperty("hasNextPage")]
    public bool HasNextPage { get; set; }

    [JsonProperty("items")]
    public List<Item> Items { get; set; }
}

public class Item
{
    [JsonProperty("vacancyReference")]
    public long VacancyReference { get; set; }

    [JsonProperty("newApplications")]
    public long NewApplications { get; set; }

    [JsonProperty("applications")]
    public long Applications { get; set; }
}