using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
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

        public async Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string accountId)
        {
            try
            {
                var entities = await GetLegalEntitiesConnectedToAccountAsync(accountId);

                return entities.Select(LegalEntityMapper.MapFromAccountApiLegalEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve account information for account Id: {accountId}");
                throw;
            }
        }

        public async Task<IEnumerable<LegalEntityViewModel>> GetLegalEntitiesConnectedToAccountAsync(string accountId)
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

        public async Task<string> GetEmployerAccountPublicHashedIdAsync(long accountId)
        {
            try
            {
                var account = await _accountApiClient.GetAccount(accountId);
                return account.HashedAccountId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve account information for account Id: {accountId}");
                throw;
            }
        }

        public async Task<EmployerAccountDetails> GetEmployerAccountDetailsAsync(string employerAccountId)
        {
            try
            {
                var account = await _accountApiClient.GetAccount(employerAccountId);
                return new EmployerAccountDetails(
                    accountAgreementType: (AccountAgreementType)account.AccountAgreementType,
                    apprenticeshipEmployerType: account.ApprenticeshipEmployerType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve account information for account Id: {employerAccountId}");
                throw;
            }
        }
    }
}