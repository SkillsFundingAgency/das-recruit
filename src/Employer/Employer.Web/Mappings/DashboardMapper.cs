using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class DashboardMapper
    {
        public static DashboardViewModel MapFromDashboard(Dashboard dashboard, string employerName)
        {
            return new DashboardViewModel
            {
                EmployerName = employerName,
                Vacancies = dashboard?.Vacancies
                                        .OrderByDescending(v => v.CreatedDate)
                                        .ToList() ?? new List<VacancySummary>()
            };
        }
    }
}
