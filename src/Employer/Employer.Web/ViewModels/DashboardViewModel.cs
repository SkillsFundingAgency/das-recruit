using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class DashboardViewModel
    {
        public IList<VacancySummary> Vacancies { get; set; }
        public string WarningMessage { get; internal set; }
        public string InfoMessage { get; internal set; }

        public bool HasVacancies => Vacancies.Any();
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
        public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);
        public bool CanCloneVacancies { get; internal set; }
    }
}
