using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class DashboardMapper
    {
        public static DashboardViewModel MapFromDashboard(Dashboard dashboard)
        {
            return new DashboardViewModel
            {
                Vacancies = dashboard?.Vacancies
                                        .OrderByDescending(v => v.CreatedDate)
                                        .ToList() ?? new List<VacancySummary>()
            };
        }
    }
}
