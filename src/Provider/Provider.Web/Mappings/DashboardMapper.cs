using System.Linq;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Provider.Web.Mappings
{
    public class DashboardMapper
    {
        public static DashboardViewModel MapFromProviderDashboard(ProviderDashboard dashboard)
        {
            return new DashboardViewModel {
                Vacancies = dashboard?.Vacancies.Select(VacancySummarVmMapper.ConvertToVacancySummaryViewModel)
                    .OrderByDescending(v => v.CreatedDate)
                    .ToList()
            };
        }
    }
}
