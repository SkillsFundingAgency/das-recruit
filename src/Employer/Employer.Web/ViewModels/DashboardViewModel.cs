using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class DashboardViewModel
    {
        public string EmployerName { get; set; }

        public IList<VacancySummary> Vacancies { get; set; }
        public string WarningMessage { get; internal set; }
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
    }
}
