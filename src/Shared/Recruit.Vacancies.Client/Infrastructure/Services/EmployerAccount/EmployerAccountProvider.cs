using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    internal class EmployerAccountProvider : IEmployerAccountProvider
    {
        private readonly ILogger<EmployerAccountProvider> _logger;
        private readonly IAccountApiClient _accountApiClient;

        public EmployerAccountProvider(ILogger<EmployerAccountProvider> logger, IAccountApiClient accountApiClient)
        {
            _logger = logger;
            _accountApiClient = accountApiClient;
        }

        public async Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId)
        {
            try
            {
                var accounts = await _accountApiClient.GetUserAccounts(userId);

                return accounts.Select(acc => acc.HashedAccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve account information for user Id: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<LegalEntityViewModel>> GetEmployerLegalEntitiesAsync(string accountId)
        {
            try
            {
                var accounts = await _accountApiClient.GetLegalEntitiesConnectedToAccount(accountId);

                var legalEntitiesTasks = accounts.Select(r => _accountApiClient.GetLegalEntity(accountId, long.Parse(r.Id)));

                await Task.WhenAll(legalEntitiesTasks.ToArray());

                var entities = legalEntitiesTasks.Select(t => t.Result);

                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve account information for account Id: {accountId}");
                throw;
            }
        }
    }
}