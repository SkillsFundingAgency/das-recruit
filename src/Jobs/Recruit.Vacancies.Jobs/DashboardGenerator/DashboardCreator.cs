using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.DashboardGenerator
{
    public class DashboardCreator
    {
        private readonly IDashboardService _dashboardService;

        public DashboardCreator(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public Task RunAsync(string employerId)
        {
            return _dashboardService.ReBuildDashboardAsync(employerId);
        }
    }
}
