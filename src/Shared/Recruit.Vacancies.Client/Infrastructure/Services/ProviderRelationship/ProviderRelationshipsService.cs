using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship
{
    public class ProviderRelationshipsService(IEmployerAccountProvider employerAccountProvider,
        IOuterApiClient outerApiClient,
        IEncodingService encodingService,
        ICache cache,
        ITimeProvider timeProvider)
        : IProviderRelationshipsService
    {
        public async Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProviderAsync(long ukprn, List<OperationType> operationTypes)
        {
            var providerPermissions = await GetProviderPermissionsByUkprn(ukprn, operationTypes);

            return await GetEmployerInfosAsync(providerPermissions);
        }

        public async Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProvider(long ukprn, string accountHashedId,
            List<OperationType> operationTypes)
        {
            var providerPermissions = await GetProviderPermissionsByUkprn(ukprn, operationTypes);

            var filteredProviderPermissions = new ProviderPermissions
            {
                AccountProviderLegalEntities = providerPermissions.AccountProviderLegalEntities
                    .Where(p => p.AccountHashedId == accountHashedId)
                    .ToList()
            };

            return await GetEmployerInfosAsync(filteredProviderPermissions);
        }

        public async Task<bool> HasProviderGotEmployersPermissionAsync(long ukprn, string accountPublicHashedId, string accountLegalEntityPublicHashedId, OperationType operationType)
        {
            var permittedLegalEntities = await GetProviderPermissionsForEmployer(ukprn, accountPublicHashedId, [operationType]);

            if (permittedLegalEntities.Count == 0) return false;

            var accountId = permittedLegalEntities[0].AccountHashedId;
            var allLegalEntities = (await employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(accountId)).ToList();

            var hasPermission = permittedLegalEntities
                .Join(allLegalEntities,
                    ple => ple.AccountLegalEntityPublicHashedId,
                    ale => ale.AccountLegalEntityPublicHashedId,
                    (_, ale) => ale)
                .Any(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);

            return hasPermission;
        }

        public async Task<bool> CheckEmployerHasPermissions(string accountHashedId, OperationType operationType)
        {
            var result = await GetProviderPermissionsByAccountHashedId(accountHashedId, operationType);

            return result.AccountProviderLegalEntities.Any();
        }

        private async Task<List<LegalEntityDto>> GetProviderPermissionsForEmployer(long ukprn, string accountHashedId, List<OperationType> operationTypes)
        {
            var accountId = encodingService.Decode(accountHashedId, EncodingType.AccountId);
            var operationsKey = string.Join(",", operationTypes
                .Select(x => x.ToString())
                .OrderBy(x => x));
           
            return await cache.CacheAsideAsync($"{CacheKeys.ProviderPermissions}_{ukprn}_{accountId}_{operationsKey.GetHashCode()}",
                timeProvider.NextDay,
                async () =>
                {
                    var retryPolicy = PollyRetryPolicy.GetPolicy();

                    var permissions = await retryPolicy.Execute(_ => outerApiClient.Get<GetProviderPermissionsByUkprnAndAccountIdApiResponse>(
                            new GetProviderPermissionsByUkprnAndAccountIdApiRequest(ukprn, accountId, operationTypes)),
                        new Dictionary<string, object>
                        {
                            {
                                "apiCall", "AccountLegalEntities"
                            }
                        });

                    return MapToLegalEntities(permissions.AccountProviderLegalEntities);
                });
        }

        private async Task<ProviderPermissions> GetProviderPermissionsByUkprn(long ukprn, List<OperationType> operationTypes)
        {
            var operationsKey = string.Join(",", operationTypes
                .Select(x => x.ToString())
                .OrderBy(x => x));
             
            return await cache.CacheAsideAsync($"{CacheKeys.ProviderPermissions}_{ukprn}_{operationsKey.GetHashCode()}",
                timeProvider.NextDay,
                async () =>
                {
                    var retryPolicy = PollyRetryPolicy.GetPolicy();
                    var permissions = await retryPolicy.Execute(_ => outerApiClient.Get<GetProviderPermissionsByUkprnApiResponse>(
                            new GetProviderPermissionsByUkprnApiRequest(ukprn, operationTypes)),
                        new Dictionary<string, object>
                        {
                            {
                                "apiCall", "AccountLegalEntities"
                            }
                        });

                    return MapToProviderPermissions(permissions.AccountProviderLegalEntities);
                });
        }

        private async Task<ProviderPermissions> GetProviderPermissionsByAccountHashedId(string accountHashedId, OperationType operationType)
        {
            var operationsKey = string.Join(",", new[] { operationType }.OrderBy(x => x));
            return await cache.CacheAsideAsync($"{CacheKeys.ProviderPermissions}_{accountHashedId}_{operationsKey.GetHashCode()}",
                timeProvider.NextDay,
                async () =>
                {
                    var retryPolicy = PollyRetryPolicy.GetPolicy();

                    var permissions = await retryPolicy.Execute(_ => outerApiClient.Get<GetProviderPermissionsByUkprnApiResponse>(
                            new GetEmployerPermissionsByAccountHashedIdApiRequest(accountHashedId, [operationType])),
                        new Dictionary<string, object>
                        {
                            {
                                "apiCall", "AccountLegalEntities"
                            }
                        });

                    return MapToProviderPermissions(permissions.AccountProviderLegalEntities);
                });
        }

        private static ProviderPermissions MapToProviderPermissions(
            List<AccountLegalEntityItem> accountLegalEntityItems) =>
            new()
            {
                AccountProviderLegalEntities = accountLegalEntityItems
                    .Select(l => new LegalEntityDto
                    {
                        AccountHashedId = l.AccountHashedId,
                        AccountLegalEntityPublicHashedId = l.AccountLegalEntityPublicHashedId,
                        AccountName = l.AccountName,
                        AccountLegalEntityId = l.AccountLegalEntityId,
                        AccountId = l.AccountId,
                        AccountLegalEntityName = l.AccountLegalEntityName,
                        AccountProviderId = l.AccountProviderId,
                        AccountPublicHashedId = l.AccountPublicHashedId,
                    })
                    .ToList()
            };

        private static List<LegalEntityDto> MapToLegalEntities(List<AccountLegalEntityItem> accountLegalEntityItems) =>
            accountLegalEntityItems
                .Select(l => new LegalEntityDto
                {
                    AccountHashedId = l.AccountHashedId,
                    AccountLegalEntityPublicHashedId = l.AccountLegalEntityPublicHashedId,
                    AccountName = l.AccountName,
                    AccountLegalEntityId = l.AccountLegalEntityId,
                    AccountId = l.AccountId,
                    AccountLegalEntityName = l.AccountLegalEntityName,
                    AccountProviderId = l.AccountProviderId,
                    AccountPublicHashedId = l.AccountPublicHashedId,
                })
                .ToList();

        private async Task<List<EmployerInfo>> GetEmployerInfosAsync(ProviderPermissions providerPermissions)
        {
            var employerInfos = new List<EmployerInfo>();

            var permittedEmployerAccounts = providerPermissions.AccountProviderLegalEntities.GroupBy(p => p.AccountHashedId);

            foreach (var permittedEmployer in permittedEmployerAccounts)
            {
                var employerInfo = new EmployerInfo
                {
                    EmployerAccountId = permittedEmployer.Key,
                    Name = permittedEmployer.First().AccountName, //should be same in all the items hence read from first
                    LegalEntities = []
                };

                var legalEntityViewModels = await employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(permittedEmployer.Key);
                var accountLegalEntities = legalEntityViewModels.ToList();
                foreach (LegalEntityDto permittedLegalEntity in permittedEmployer)
                {
                    if (accountLegalEntities.Count <= 0) continue;
                    
                    var matchingLegalEntity = accountLegalEntities.FirstOrDefault(e => e.AccountLegalEntityPublicHashedId == permittedLegalEntity.AccountLegalEntityPublicHashedId);

                    if (matchingLegalEntity == null) continue;
                    
                    var legalEntity = LegalEntityMapper.MapFromAccountApiLegalEntity(matchingLegalEntity);
                    legalEntity.AccountLegalEntityPublicHashedId = permittedLegalEntity.AccountLegalEntityPublicHashedId;
                    employerInfo.LegalEntities.Add(legalEntity);
                }

                employerInfos.Add(employerInfo);
            }
            return employerInfos;
        }
    }
}