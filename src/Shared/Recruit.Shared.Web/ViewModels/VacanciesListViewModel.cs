using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Shared.Web.ViewModels;

public class VacanciesListViewModel
{
    public string EditVacancyRoute { get; set; }
    public string ManageVacancyRoute { get; set; }
    public PaginationViewModel Pagination { get; set; }
    public required Dictionary<string, string> RouteDictionary { get; set; }
    public bool ShowEmployerReviewedApplicationCounts { get; set; }
    public bool ShowNewApplicationCounts => !ShowEmployerReviewedApplicationCounts;
    public bool ShowSourceOrigin { get; set; }
    public string SubmitVacancyRoute { get; set; }
    public List<VacancyListItemViewModel> Vacancies { get; set; }
    public OwnerType ViewType { get; set; }
    public VacancySortColumn? SortColumn { get; set; }
    public ColumnSortOrder? SortOrder { get; set; }
}