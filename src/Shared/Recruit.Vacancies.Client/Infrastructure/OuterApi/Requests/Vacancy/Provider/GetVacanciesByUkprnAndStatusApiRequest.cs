using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy.Provider;

public record GetVacanciesByUkprnAndStatusApiRequest(int Ukprn,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 25,
    FilteringOptions FilteringOption = FilteringOptions.All,
    VacancySortColumn? SortColumn = VacancySortColumn.CreatedDate,
    ColumnSortOrder? SortOrder = ColumnSortOrder.Desc) : GetVacanciesByUkprnApiRequestV2(FilteringOption.ToString(), Ukprn, SearchTerm, Page, PageSize, SortColumn, SortOrder);
