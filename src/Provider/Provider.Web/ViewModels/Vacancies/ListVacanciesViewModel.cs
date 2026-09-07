using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Vacancies;

public class ListVacanciesViewModel
{
    public string WarningMessage { get; set; }
    public string InfoMessage { get; set; }
    public required string PageHeading { get; set; }
    public int Ukprn { get; set; }
    public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
    public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);
    public required VacanciesListViewModel ListViewModel { get; set; }
    public required VacanciesListSearchFilterViewModel FilterViewModel { get; set; }
    public string NoResultsHeadingText { get; set; }
    public string NoResultsLabelText { get; set; }
    public FilteringOptions FilteringOptions { get; set; }
}