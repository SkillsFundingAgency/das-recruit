using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Vacancies;

public class ListVacanciesViewModel
{
    public AlertsViewModel Alerts { get; set; }
    public string WarningMessage { get; set; }
    public string InfoMessage { get; set; }
    public required string PageHeading { get; set; }
    public string EmployerAccountId { get; set; }
    public uint TotalVacancies { get; set; }
    public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
    public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);
    public required VacanciesListViewModel ListViewModel { get; set; }
    public required VacanciesListSearchFilterViewModel FilterViewModel { get; set; }
    public bool ShowReferredFromMaBackLink { get; set; }
}