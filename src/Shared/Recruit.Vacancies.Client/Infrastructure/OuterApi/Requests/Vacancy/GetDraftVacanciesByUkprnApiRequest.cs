using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy;

public sealed record GetDraftVacanciesByUkprnApiRequest(
    int Ukprn,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 25,
    VacancySortColumn? SortColumn = VacancySortColumn.CreatedDate,
    ColumnSortOrder? SortOrder = ColumnSortOrder.Desc) : GetVacanciesByUkprnApiRequestV2("draft", Ukprn, SearchTerm, Page, PageSize, SortColumn, SortOrder);