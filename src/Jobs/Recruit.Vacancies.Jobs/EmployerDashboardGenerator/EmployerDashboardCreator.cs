using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.EmployerDashboardGenerator
{
    public class EmployerDashboardCreator
    {
        private readonly IDashboardService _dashboardService;

        public EmployerDashboardCreator(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public Task RunAsync(string employerId)
        {
            return _dashboardService.ReBuildDashboardAsync(employerId);
        }
    }
}
