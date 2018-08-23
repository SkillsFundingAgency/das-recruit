using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.EmployerDashboardGenerator
{
    public class EmployerDashboardCreator
    {
        private readonly IEmployerDashboardService _dashboardService;

        public EmployerDashboardCreator(IEmployerDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public Task RunAsync(string employerId)
        {
            return _dashboardService.ReBuildDashboardAsync(employerId);
        }
    }
}
