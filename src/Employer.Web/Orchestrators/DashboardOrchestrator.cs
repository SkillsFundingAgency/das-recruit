using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IVacancyClient _vacancyClient;
        private readonly IEmployerAccountService _getAccountService;
        private readonly DashboardMapper _mapper = new DashboardMapper();

        public DashboardOrchestrator(IEmployerAccountService getAccountsService, IVacancyClient vacancyClient)
        {
            _getAccountService = getAccountsService;
            _vacancyClient = vacancyClient;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string employerAccountId)
        {
            var account = _getAccountService.GetAccountDetailAsync(employerAccountId);
            var dashboard = _vacancyClient.GetDashboardAsync(employerAccountId);
            await Task.WhenAll(account, dashboard);

            var vm = _mapper.MapFromDashboard(dashboard.Result, account.Result);

            return vm;
        }
    }
}