using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy.Employer;

public record GetVacanciesByEmployerAccountAndStatusApiRequest(long EmployerAccountId,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 25,
    FilteringOptions FilteringOption = FilteringOptions.All,
    VacancySortColumn? SortColumn = VacancySortColumn.CreatedDate,
    ColumnSortOrder? SortOrder = ColumnSortOrder.Desc) : GetVacanciesByEmployerAccountApiRequestV2(FilteringOption.ToString(), EmployerAccountId, SearchTerm, Page, PageSize, SortColumn, SortOrder);
