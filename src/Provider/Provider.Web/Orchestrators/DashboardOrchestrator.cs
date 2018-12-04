using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IEmployerVacancyClient _vacancyClient;

        public DashboardOrchestrator(IEmployerVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string ukprn)
        {            
            var dashboard = await _vacancyClient.GetDashboardAsync(ukprn);

            if (dashboard == null)
            {
                await _vacancyClient.GenerateDashboard(ukprn);
                dashboard = await _vacancyClient.GetDashboardAsync(ukprn);
            }

            var vm = DashboardMapper.MapFromEmployerDashboard(dashboard);
            return vm;
        }
    }
}