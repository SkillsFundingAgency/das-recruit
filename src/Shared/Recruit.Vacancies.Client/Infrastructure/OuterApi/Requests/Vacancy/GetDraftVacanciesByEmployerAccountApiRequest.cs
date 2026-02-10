using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy;

public record GetDraftVacanciesByEmployerAccountApiRequest(
    long EmployerAccountId,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 25,
    VacancySortColumn? SortColumn = VacancySortColumn.CreatedDate,
    ColumnSortOrder? SortOrder = ColumnSortOrder.Desc) : GetVacanciesByEmployerAccountApiRequestV2("draft", EmployerAccountId, SearchTerm, Page, PageSize, SortColumn, SortOrder);