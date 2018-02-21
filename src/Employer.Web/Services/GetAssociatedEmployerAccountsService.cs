using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IAccountApiConfiguration _accountApiConfig;
        private readonly AccountApiClient _client;

        public GetAssociatedEmployerAccountsService(ILogger<GetAssociatedEmployerAccountsService> logger, IOptions<AccountApiConfiguration> accountApiConfig)
        {
            _logger = logger;
            _accountApiConfig = accountApiConfig.Value;
            _client = new AccountApiClient(_accountApiConfig);
        }

        public async Task<string[]> GetAssociatedAccounts(string userId)
        {
            try
            {
                var accounts = await _client.GetUserAccounts(userId);
                
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
                return await _client.GetAccount(employerAccountId);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failure connecting to Accounts API");
                throw;
            }
        }
    }
}
