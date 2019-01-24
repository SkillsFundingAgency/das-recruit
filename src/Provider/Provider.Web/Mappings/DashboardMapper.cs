using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Provider.Web.Mappings
{
    public class DashboardMapper
    {
        public static DashboardViewModel MapFromProviderDashboard(ProviderDashboard dashboard)
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
