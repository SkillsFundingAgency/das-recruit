using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;

namespace Esfa.Recruit.Provider.Web.Mappings
{
    public class DashboardMapper
    {
        public static DashboardViewModel MapFromEmployerDashboard(EmployerDashboard dashboard)
        {
            return new DashboardViewModel
            {
            };
        }
    }
}
