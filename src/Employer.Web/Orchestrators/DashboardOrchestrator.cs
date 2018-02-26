using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;
using System.Linq;
using System.Threading.Tasks;

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
                Vacancies = vacancies.Result.OrderByDescending(v => v.CreatedDate).ToList()
            };

            return vm;
        }
    }
}