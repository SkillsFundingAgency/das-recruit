using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public class EmployerAccountProvider : IEmployerAccountProvider
    {
        private readonly ILogger<EmployerAccountProvider> _logger;
        private readonly IOuterApiClient _outerApiClient;

        public EmployerAccountProvider(ILogger<EmployerAccountProvider> logger, IOuterApiClient outerApiClient)
        {
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<GetUserAccountsResponse> GetEmployerIdentifiersAsync(string userId, string email)
        {
            try
            {
                var response = await _outerApiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(userId, email));
                
                return response;
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

        public async Task<IEnumerable<AccountLegalEntity>> GetLegalEntitiesConnectedToAccountAsync(string accountId)
        {
            try
            {
                var legalEntities =
                    await _outerApiClient.Get<GetAccountLegalEntitiesResponse>(
                        new GetAccountLegalEntitiesRequest(accountId));
                
                return legalEntities.AccountLegalEntities;
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
                var account = await _outerApiClient.Get<GetAccountResponse>(new GetAccountRequest(accountId));
                
                return account.HashedAccountId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve account information for account Id: {accountId}");
                throw;
            }
        }
    }
}