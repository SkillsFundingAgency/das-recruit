using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.EmployerDashboardGenerator
{
    public class EmployerDashboardCreator
    {
        private readonly IEmployerDashboardProjectionService _projectionService;

        public EmployerDashboardCreator(IEmployerDashboardProjectionService dashboardService)
        {
            _projectionService = dashboardService;
        }

        public Task RunAsync(string employerId)
        {
            return _projectionService.ReBuildDashboardAsync(employerId);
        }
    }
}
