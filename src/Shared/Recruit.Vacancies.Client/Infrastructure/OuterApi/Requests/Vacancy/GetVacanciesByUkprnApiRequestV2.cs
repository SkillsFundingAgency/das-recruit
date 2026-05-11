using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy;

public abstract record GetVacanciesByUkprnApiRequestV2(
    string Endpoint,
    int Ukprn,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 25,
    VacancySortColumn? SortColumn = VacancySortColumn.CreatedDate,
    ColumnSortOrder? SortOrder = ColumnSortOrder.Desc) : IGetApiRequest
{
    public string GetUrl
    {
        get
        {
            var baseUrl = $"vacancies/provider/{Ukprn}/{Endpoint}";
            var queryParams = new Dictionary<string, string>
            {
                ["pageNumber"] = $"{Page}",
                ["pageSize"] = $"{PageSize}",
                ["sortOrder"] = $"{SortOrder}",
                ["sortColumn"] = $"{SortColumn}",
                ["searchTerm"] = $"{SearchTerm}",
            };
            
            return QueryHelpers.AddQueryString(baseUrl, queryParams);
        }
    } 
}