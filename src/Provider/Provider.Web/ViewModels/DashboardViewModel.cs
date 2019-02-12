using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class DashboardViewModel
    {
        public IList<VacancySummaryViewModel> Vacancies { get; set; }
        public string WarningMessage { get; internal set; }
        public string InfoMessage { get; internal set; }

        public bool HasVacancies => Vacancies.Any();
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
        public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);
    }
}
