using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;
using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;
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
            var dashboard = _queryRepository.GetDashboardAsync(employerAccountId);
            await Task.WhenAll(account, dashboard);

            var vm = MapToDashboardViewModel(dashboard.Result, account.Result);

            return vm;
        }

        private DashboardViewModel MapToDashboardViewModel(Dashboard dashboard, AccountDetailViewModel accountDetail)
        {
            return new DashboardViewModel
            {
                EmployerName = accountDetail.DasAccountName,
                Vacancies = dashboard?.Vacancies
                                        .OrderByDescending(v => v.CreatedDate)
                                        .ToList() ?? new List<VacancySummary>()
            };
        }
    }
}