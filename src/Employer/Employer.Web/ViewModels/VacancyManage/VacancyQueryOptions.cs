using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;

public sealed record VacancyQueryOptions(
    int PageNumber,
    int PageSize,
    SortColumn SortColumn,
    SortOrder SortOrder,
    string LocationFilter = "All",
    string ApplicantFilter = "");
