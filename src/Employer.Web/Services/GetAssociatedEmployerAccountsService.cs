using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class GetAssociatedEmployerAccountsService : IGetAssociatedEmployerAccountsService
    {
        private readonly ILogger<GetAssociatedEmployerAccountsService> _logger;
        private readonly IAccountApiClient _accountApiClient;

        public GetAssociatedEmployerAccountsService(ILogger<GetAssociatedEmployerAccountsService> logger, IAccountApiClient accountApiClient)
        {
            _logger = logger;
            _accountApiClient = accountApiClient;
        }

        public async Task<string[]> GetAssociatedAccountsAsync(string userId)
        {
            try
            {
                var accounts = await _accountApiClient.GetUserAccounts(userId);
                
                return accounts.ToList().Select(acc => acc.HashedAccountId).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failure connecting to Accounts API");
                throw;
            }
        }

        public async Task<AccountDetailViewModel> GetEmployerAccountAsync(string employerAccountId)
        {
            try
            {
                return await _accountApiClient.GetAccount(employerAccountId);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failure connecting to Accounts API");
                throw;
            }
        }
    }
}
