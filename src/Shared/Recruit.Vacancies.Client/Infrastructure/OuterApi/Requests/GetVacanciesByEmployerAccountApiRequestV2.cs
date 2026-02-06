using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetVacanciesByEmployerAccountApiRequestV2(
    long EmployerAccountId,
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
            var baseUrl = $"vacancies/employer/{EmployerAccountId}/all";
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