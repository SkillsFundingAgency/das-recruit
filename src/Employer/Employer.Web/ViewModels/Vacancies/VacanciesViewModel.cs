using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Vacancies
{
    public class VacanciesViewModel
    {
        public IList<VacancySummaryViewModel> Vacancies { get; internal set; }
        public string WarningMessage { get; internal set; }
        public string InfoMessage { get; internal set; }
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
        public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);
        public PagerViewModel Pager { get; internal set; }
        public string ResultsHeading { get; internal set; }
        public FilteringOptions Filter { get; set; }
        public AlertsViewModel Alerts { get; set; }
        public string SearchTerm { get; set; }
        public bool ShowResultsTable => Vacancies.Any();
        public bool IsFiltered => Filter != FilteringOptions.All;
        public bool ShowReferredFromMaBackLink { get; set; }
    }
}
