using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Vacancies;

public class ListAllVacanciesViewModel
{
    public AlertsViewModel Alerts { get; set; }
    public string WarningMessage { get; set; }
    public string InfoMessage { get; set; }
    public string ResultsHeading { get; set; }
    public string SearchTerm { get; set; }
    public long Ukprn { get; set; }
    public uint TotalVacancies { get; set; }
    public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
    public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);
    public required VacanciesListViewModel ListViewModel { get; set; }
}