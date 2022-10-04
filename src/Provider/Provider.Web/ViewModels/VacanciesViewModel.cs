using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class VacanciesViewModel
    {
        public IList<VacancySummaryViewModel> Vacancies { get; set; }
        public AlertsViewModel Alerts { get; internal set; }
        public string WarningMessage { get; internal set; }
        public string InfoMessage { get; internal set; }
        public PagerViewModel Pager { get; internal set; }
        public string ResultsHeading { get; internal set; }
        public bool IsFiltered => Filter != FilteringOptions.All;
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
        public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);
        public bool ShowResultsTable => Vacancies.Any();
        public FilteringOptions Filter { get; set; }
        public string SearchTerm { get; set; }
        public bool HasEmployerReviewPermission { get; set; }
        public long Ukprn { get; set; }
        public int TotalVacancies { get; set; }
    }
}
