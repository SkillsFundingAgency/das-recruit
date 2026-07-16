using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        IEncodingService encodingService)
        : IProviderRelationshipsService
    {
        private readonly Dictionary<string, object> _apiLoggingContext = new()
        {
            {
                "apiCall", "AccountLegalEntities"
            }
        };

        public async Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProvider(long ukprn, string accountHashedId,
            List<OperationType> operationTypes)
        {
            var providerPermissions = await GetProviderPermissionsByUkprn(ukprn, operationTypes);

            var filteredProviderPermissions = !string.IsNullOrEmpty(accountHashedId) ? new ProviderPermissions
            {
                AccountProviderLegalEntities = providerPermissions.AccountProviderLegalEntities
                    .Where(p => p.AccountHashedId == accountHashedId)
                    .ToList()
            } : providerPermissions;

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

            var retryPolicy = PollyRetryPolicy.GetPolicy();

            var permissions = await retryPolicy.Execute(_ => outerApiClient.Get<GetProviderPermissionsByUkprnAndAccountIdApiResponse>(
                    new GetProviderPermissionsByUkprnAndAccountIdApiRequest(ukprn, accountId, operationTypes)),
                _apiLoggingContext);

            return MapToLegalEntities(permissions.AccountProviderLegalEntities);
        }

        private async Task<ProviderPermissions> GetProviderPermissionsByUkprn(long ukprn, List<OperationType> operationTypes)
        {
            var permissions = await outerApiClient.Get<GetProviderPermissionsByUkprnApiResponse>(
                    new GetProviderPermissionsByUkprnApiRequest(ukprn, operationTypes));

            return MapToProviderPermissions(permissions.AccountProviderLegalEntities);
        }

        private async Task<ProviderPermissions> GetProviderPermissionsByAccountHashedId(string accountHashedId, OperationType operationType)
        {
            var retryPolicy = PollyRetryPolicy.GetPolicy();

            var permissions = await retryPolicy.Execute(_ => outerApiClient.Get<GetProviderPermissionsByUkprnApiResponse>(
                    new GetEmployerPermissionsByAccountHashedIdApiRequest(accountHashedId, [operationType])),
                _apiLoggingContext);

            return MapToProviderPermissions(permissions.AccountProviderLegalEntities);
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

        private const int BatchSize = 50;
        private const int PageSize = 500;

        private async Task<List<EmployerInfo>> GetEmployerInfosAsync(ProviderPermissions providerPermissions)
        {
            var employerInfos = new List<EmployerInfo>();

            var permittedEmployerAccounts = providerPermissions.AccountProviderLegalEntities
                .GroupBy(p => p.AccountHashedId)
                .ToList();

            foreach (var batch in permittedEmployerAccounts.Chunk(BatchSize))
            {
                var accountIds = batch.Select(g => g.Key).ToList();

                var firstPage = await employerAccountProvider.GetAllLegalEntitiesConnectedToAccountAsync(accountIds, string.Empty, 1, PageSize, "Name", true);
                var allLegalEntities = firstPage.LegalEntities.ToList();

                for (var page = 2; page <= firstPage.PageInfo.TotalPages; page++)
                {
                    var nextPage = await employerAccountProvider.GetAllLegalEntitiesConnectedToAccountAsync(accountIds, string.Empty, page, PageSize, "Name", true);
                    allLegalEntities.AddRange(nextPage.LegalEntities);
                }

                var legalEntitiesByPublicHashedId = allLegalEntities
                    .ToDictionary(le => le.AccountLegalEntityPublicHashedId);

                foreach (var permittedEmployer in batch)
                {
                    var employerInfo = new EmployerInfo
                    {
                        EmployerAccountId = permittedEmployer.Key,
                        Name = permittedEmployer.First().AccountName,
                        LegalEntities = []
                    };

                    foreach (LegalEntityDto permittedLegalEntity in permittedEmployer)
                    {
                        if (!legalEntitiesByPublicHashedId.TryGetValue(permittedLegalEntity.AccountLegalEntityPublicHashedId, out var matchingLegalEntity))
                            continue;

                        var legalEntity = LegalEntityMapper.MapFromAllAccountApiLegalEntity(matchingLegalEntity);
                        legalEntity.AccountLegalEntityPublicHashedId = permittedLegalEntity.AccountLegalEntityPublicHashedId;
                        employerInfo.LegalEntities.Add(legalEntity);
                    }

                    employerInfos.Add(employerInfo);
                }
            }

            return employerInfos;
        }
    }
}