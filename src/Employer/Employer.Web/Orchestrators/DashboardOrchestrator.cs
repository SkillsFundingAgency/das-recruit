using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IEmployerVacancyClient _vacancyClient;

        public DashboardOrchestrator(IEmployerVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string employerAccountId)
        {            
            var dashboard = await _vacancyClient.GetDashboardAsync(employerAccountId);
            var vm = DashboardMapper.MapFromEmployerDashboard(dashboard);
            return vm;
        }
    }
}