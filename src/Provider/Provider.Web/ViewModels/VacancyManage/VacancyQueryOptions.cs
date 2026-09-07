using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyManage;

public record VacancyQueryOptions(
    int PageNumber,
    int PageSize,
    SortColumn SortColumn,
    SortOrder SortOrder,
    string LocationFilter = "All",
    string ApplicantFilter = "");