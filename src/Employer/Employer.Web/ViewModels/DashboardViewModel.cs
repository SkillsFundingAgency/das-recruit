using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class DashboardViewModel
    {
        public IList<VacancySummary> Vacancies { get; set; }
        public string WarningMessage { get; internal set; }

        public bool ShowNoVacanciesMessage => Vacancies.Count == 0;
        public bool HasVacancies => Vacancies.Any();
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
    }
}
