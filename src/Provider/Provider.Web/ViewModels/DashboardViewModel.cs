using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class DashboardViewModel
    {
        public IList<VacancySummaryViewModel> Vacancies { get; set; }
        public string WarningMessage { get; internal set; }
        public string InfoMessage { get; internal set; }
        public PagerViewModel Pager { get; internal set; }
        public IEnumerable<SelectListItem> FilterOptions { get; internal set; }        
        public string ResultsHeading { get; internal set; }
        public bool IsFiltered { get; internal set; }
        public bool HasVacancies {get; internal set;}
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
        public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);
        public bool ShowResultsTable => Vacancies.Any();        
        public string Filter { get; set; }
    }
}
