using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using System.Linq;
using Esfa.Recruit.Shared.Web.Mappers;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class DashboardMapper
    {
        public static DashboardViewModel MapFromEmployerDashboard(EmployerDashboard dashboard)
        {
            return new DashboardViewModel {
                Vacancies = dashboard?.Vacancies.Select(VacancySummarVmMapper.ConvertToVacancySummaryViewModel)
                    .OrderByDescending(v => v.CreatedDate)
                    .ToList()
            };
        }
    }
}
