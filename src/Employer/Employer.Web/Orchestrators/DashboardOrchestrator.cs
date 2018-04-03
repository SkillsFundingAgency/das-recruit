using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IVacancyClient _vacancyClient;
        private readonly IEmployerAccountService _getAccountService;

        public DashboardOrchestrator(IEmployerAccountService getAccountsService, IVacancyClient vacancyClient)
        {
            _getAccountService = getAccountsService;
            _vacancyClient = vacancyClient;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(EmployerIdentifier employerDetail)
        {            
            var dashboard = await _vacancyClient.GetDashboardAsync(employerDetail.AccountId);
            var vm = DashboardMapper.MapFromDashboard(dashboard);
            return vm;
        }
    }
}