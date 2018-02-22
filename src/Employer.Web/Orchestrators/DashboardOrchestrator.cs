using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IQueryStoreReader _queryRepository;
        private readonly IEmployerAccountService _getAccountService;

        public DashboardOrchestrator(IEmployerAccountService getAccountsService, IQueryStoreReader queryRepository)
        {
            _getAccountService = getAccountsService;
            _queryRepository = queryRepository;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string employerAccountId)
        {
            var account = _getAccountService.GetAccountDetailAsync(employerAccountId);
            var vacancies = _queryRepository.GetVacanciesAsync(employerAccountId);
            await Task.WhenAll(account, vacancies);
            
            var vm = new DashboardViewModel
            {
                EmployerName = account.Result.DasAccountName,
                Vacancies = vacancies.Result.OrderByDescending(v => v.AuditVacancyCreated).ToList()
            };

            return vm;
        }
    }
}