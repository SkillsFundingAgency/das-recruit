using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship
{
    public class ProviderRelationshipsService : IProviderRelationshipsService
    {
        private const string OperationsQueryStringKey = "Operations";
        private const string UkprnQueryStringKey = "Ukprn";
        private readonly ILogger<ProviderRelationshipsService> _logger;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private readonly HttpClient _httpClient;

        public ProviderRelationshipsService(
            ILogger<ProviderRelationshipsService> logger,
            IEmployerAccountProvider employerAccountProvider,
            HttpClient httpClient)
        {
            _logger = logger;
            _employerAccountProvider = employerAccountProvider;
            _httpClient = httpClient;
        }

        public async Task RevokeProviderPermissionToRecruitAsync(long ukprn, string accountLegalEntityPublicHashedId)
        {
            var stringContent = GetStringContent(ukprn, accountLegalEntityPublicHashedId);

            var response = await _httpClient.PostAsync("/permissions/revoke", stringContent);

            if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.NotModified)
            {
                throw new InvalidOperationException($"Failed to revoke provider {ukprn} permission for account legal entity {accountLegalEntityPublicHashedId} response code: {response.StatusCode}");
            }
        }

        public async Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProviderAsync(long ukprn, OperationType operationType)
        {
            var providerPermissions = await GetProviderPermissionsByUkprn(ukprn, operationType);

            return await GetEmployerInfosAsync(providerPermissions);
        }

        public async Task<bool> HasProviderGotEmployersPermissionAsync(long ukprn, string accountPublicHashedId, string accountLegalEntityPublicHashedId, OperationType operationType)
        {
            var queryData = new List<KeyValuePair<string, string>>
            {
                new(OperationsQueryStringKey, operationType.ToString()),
                new(UkprnQueryStringKey, ukprn.ToString())
            };
            var providerPermissions = await GetProviderPermissions(queryData);

            var permittedLegalEntities = providerPermissions.AccountProviderLegalEntities
                .Where(l => l.AccountHashedId == accountPublicHashedId)
                .ToList();

            if (permittedLegalEntities.Count == 0) return false;

            var accountId = permittedLegalEntities[0].AccountHashedId;
            var allLegalEntities = (await _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(accountId)).ToList();

            bool hasPermission = permittedLegalEntities
                .Join(allLegalEntities,
                    ple => ple.AccountLegalEntityPublicHashedId,
                    ale => ale.AccountLegalEntityPublicHashedId,
                    (ple, ale) => ale)
                .Any(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);

            return hasPermission;
        }

        public async Task<bool> CheckProviderHasPermissions(long ukprn, OperationType operationType)
        {
            var result = await GetProviderPermissionsByUkprn(ukprn, operationType);

            return result.AccountProviderLegalEntities.Any();
        }

        public async Task<bool> CheckEmployerHasPermissions(string accountHashedId, OperationType operationType)
        {
            var result = await GetProviderPermissionsByAccountHashedId(accountHashedId, operationType);

            return result.AccountProviderLegalEntities.Any();
        }

        private async Task<ProviderPermissions> GetProviderPermissionsByUkprn(long ukprn, OperationType operationType)
        {
            var queryData = InitialiseQueryDataWithOperation(operationType);
            queryData.Add(new(UkprnQueryStringKey, ukprn.ToString()));
            return await GetProviderPermissions(queryData);
        }

        private async Task<ProviderPermissions> GetProviderPermissionsByAccountHashedId(string accountHashedId, OperationType operationType)
        {
            var queryData = InitialiseQueryDataWithOperation(operationType);
            queryData.Add(new("AccountHashedId", accountHashedId));
            return await GetProviderPermissions(queryData);
        }

        private static List<KeyValuePair<string, string>> InitialiseQueryDataWithOperation(OperationType operationType)
        {
            var queryData = new List<KeyValuePair<string, string>>();
            queryData.Add(new KeyValuePair<string, string>(OperationsQueryStringKey, operationType.ToString()));
            if (operationType == OperationType.RecruitmentRequiresReview)
            {
                queryData.Add(new KeyValuePair<string, string>(OperationsQueryStringKey, OperationType.Recruitment.ToString()));
            }
            return queryData;
        }

        private async Task<ProviderPermissions> GetProviderPermissions(List<KeyValuePair<string, string>> queryData)
        {
            var uri = new Uri(QueryHelpers.AddQueryString("/accountproviderlegalentities", queryData), UriKind.Relative);

            try
            {
                var response = await _httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var providerPermissions = JsonConvert.DeserializeObject<ProviderPermissions>(content);
                    return providerPermissions;
                }

                _logger.LogError("An invalid response received when trying to get provider relationships. Status:{StatusCode} Reason:{ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error trying to retrieve legal entities.");
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Couldn't deserialise ProviderPermissions.");
            }

            return new ProviderPermissions { AccountProviderLegalEntities = Enumerable.Empty<LegalEntityDto>() };
        }

        private StringContent GetStringContent(long ukprn, string accountLegalEntityPublicHashedId)
        {
            var recruitOperationId = 1;
            var operationsToRevoke = new[] { recruitOperationId };
            var serializedData = JsonConvert.SerializeObject(new { ukprn, accountLegalEntityPublicHashedId, operationsToRevoke });
            return new StringContent(serializedData, Encoding.UTF8, "application/json");
        }

        private async Task<List<EmployerInfo>> GetEmployerInfosAsync(ProviderPermissions providerPermissions)
        {
            var employerInfos = new List<EmployerInfo>();

            var permittedEmployerAccounts = providerPermissions.AccountProviderLegalEntities.GroupBy(p => p.AccountHashedId);

            foreach (var permittedEmployer in permittedEmployerAccounts)
            {
                var employerInfo = new EmployerInfo()
                {
                    EmployerAccountId = permittedEmployer.Key,
                    Name = permittedEmployer.First().AccountName, //should be same in all the items hence read from first
                    LegalEntities = new List<LegalEntity>()
                };

                var legalEntityViewModels = await _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(permittedEmployer.Key);

                foreach (LegalEntityDto permittedLegalEntity in permittedEmployer)
                {
                    var matchingLegalEntity = legalEntityViewModels.FirstOrDefault(e => e.AccountLegalEntityPublicHashedId == permittedLegalEntity.AccountLegalEntityPublicHashedId);
                    if (matchingLegalEntity != null)
                    {
                        var legalEntity = LegalEntityMapper.MapFromAccountApiLegalEntity(matchingLegalEntity);
                        legalEntity.AccountLegalEntityPublicHashedId = permittedLegalEntity.AccountLegalEntityPublicHashedId;
                        employerInfo.LegalEntities.Add(legalEntity);
                    }
                }

                employerInfos.Add(employerInfo);
            }
            return employerInfos;
        }
    }
}
