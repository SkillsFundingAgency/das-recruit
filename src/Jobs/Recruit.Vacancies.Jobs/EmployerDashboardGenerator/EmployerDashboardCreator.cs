using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.EmployerDashboardGenerator
{
    public class EmployerDashboardCreator
    {
        private readonly IEmployerDashboardProjectionService _dashboardService;

        public EmployerDashboardCreator(IEmployerDashboardProjectionService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public Task RunAsync(string employerId)
        {
            return _dashboardService.ReBuildDashboardAsync(employerId);
        }
    }
}
