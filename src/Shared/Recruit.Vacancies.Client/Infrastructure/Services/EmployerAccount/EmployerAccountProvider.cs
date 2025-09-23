using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public class EmployerAccountProvider(
        ILogger<EmployerAccountProvider> logger,
        IOuterApiClient outerApiClient,
        IEncodingService encodingService)
        : IEmployerAccountProvider
    {
        public async Task<GetUserAccountsResponse> GetEmployerIdentifiersAsync(string userId, string email)
        {
            try
            {
                var response = await outerApiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(userId, email));
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve account information for user Id: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string hashedAccountId)
        {
            try
            {
                var entities = await GetLegalEntitiesConnectedToAccountAsync(hashedAccountId);

                return entities.Select(LegalEntityMapper.MapFromAccountApiLegalEntity).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve account information for account Id: {hashedAccountId}");
                throw;
            }
        }

        public async Task<IEnumerable<AccountLegalEntity>> GetLegalEntitiesConnectedToAccountAsync(string hashedAccountId)
        {
            try
            {
                var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
                var legalEntities =
                    await outerApiClient.Get<GetAccountLegalEntitiesResponse>(
                        new GetAccountLegalEntitiesRequest(accountId));
                
                return legalEntities.AccountLegalEntities;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve account information for account Id: {hashedAccountId}");
                throw;
            }
        }

        public async Task<GetAllAccountLegalEntitiesApiResponse> GetAllLegalEntitiesConnectedToAccountAsync(
            List<string> hashedAccountIds,
            string searchTerm,
            int pageNumber,
            int pageSize,
            string sortColumn,
            bool isAscending)
        {
            try
            {
                var accountIds = hashedAccountIds
                    .Select(hashedAccountId => encodingService.Decode(hashedAccountId, EncodingType.AccountId))
                    .ToList();

                var legalEntities =
                    await outerApiClient.Post<GetAllAccountLegalEntitiesApiResponse>(
                        new GetAllAccountLegalEntitiesApiRequest(new GetAllAccountLegalEntitiesApiRequest.GetAllAccountLegalEntitiesApiRequestData
                        {
                            SearchTerm = searchTerm,
                            AccountIds = accountIds,
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            SortColumn = sortColumn,
                            IsAscending = isAscending
                        }));

                return legalEntities;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve legal entities information");
                throw;
            }
        }

        public async Task<string> GetEmployerAccountPublicHashedIdAsync(string hashedAccountId)
        {
            try
            {
                long accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
                var account = await outerApiClient.Get<GetAccountResponse>(new GetAccountRequest(accountId));
                
                return account.HashedAccountId;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve account information for account Id: {hashedAccountId}");
                throw;
            }
        }

        public async Task<GetApplicationReviewStatsResponse> GetEmployerDashboardApplicationReviewStats(string hashedAccountId, List<long> vacancyReferences, string applicationSharedFilteringStatus)
        {
            try
            {
                logger.LogTrace("Getting Employer Application Review Stats from Outer Api");

                long accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
                var retryPolicy = PollyRetryPolicy.GetPolicy();

                return await retryPolicy.Execute(_ => outerApiClient.Post<GetApplicationReviewStatsResponse>(
                        new GetEmployerApplicationReviewsCountApiRequest(accountId,
                            vacancyReferences,applicationSharedFilteringStatus)),
                    new Dictionary<string, object>
                    {
                        {
                            "apiCall", "Providers"
                        }
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve account information for account Id: {hashedAccountId}", hashedAccountId);
                throw;
            }
        }

        public async Task<GetEmployerDashboardApiResponse> GetEmployerDashboardStats(string hashedAccountId, string userId)
        {
            logger.LogTrace("Getting Employer Dashboard Stats from Outer Api");

            long accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
            var retryPolicy = PollyRetryPolicy.GetPolicy();

            return await retryPolicy.Execute(_ => outerApiClient.Get<GetEmployerDashboardApiResponse>(
                    new GetEmployerDashboardCountApiRequest(accountId, userId)),
                new Dictionary<string, object>
                {
                    {
                        "apiCall", "Employers"
                    }
                });
        }
        public async Task<GetVacanciesDashboardResponse> GetEmployerVacancyDashboardStats(string hashedAccountId,
            List<ApplicationReviewStatus> statuses,
            int pageNumber = 1,
            int pageSize = 25)
        {
            var retryPolicy = PollyRetryPolicy.GetPolicy();
            long accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);

            return await retryPolicy.Execute(_ => outerApiClient.Get<GetVacanciesDashboardResponse>(
                    new GetEmployerDashboardVacanciesApiRequest(accountId, pageNumber, pageSize, statuses)),
                new Dictionary<string, object>
                {
                    {
                        "apiCall", "Employers"
                    }
                });
        }
    }
}