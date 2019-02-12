using System.Linq;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Provider.Web.Mappings
{
    public static class DashboardMapper
    {
        public static DashboardViewModel MapFromProviderDashboard(ProviderDashboard dashboard)
        {
            return new DashboardViewModel
            {
                Vacancies = dashboard?.Vacancies.Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
                                                .OrderByDescending(v => v.CreatedDate)
                                                .ToList()
            };
        }
    }
}
