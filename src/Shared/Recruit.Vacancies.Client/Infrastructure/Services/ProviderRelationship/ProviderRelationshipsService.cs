using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship
{
    public class ProviderRelationshipsService : IProviderRelationshipsService
    {
        private readonly ProviderRelationshipApiConfiguration _configuration;
        private readonly ILogger<ProviderRelationshipApiConfiguration> _logger;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private readonly IAccountApiClient _accountApiClient;

        public ProviderRelationshipsService(IOptions<ProviderRelationshipApiConfiguration> configuration,
            ILogger<ProviderRelationshipApiConfiguration> logger,
            IEmployerAccountProvider employerAccountProvider,
            IAccountApiClient accountApiClient)
        {
            _configuration = configuration.Value;
            _logger = logger;
            _employerAccountProvider = employerAccountProvider;
            _accountApiClient = accountApiClient;
        }

        public async Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProviderAsync(long ukprn, OperationType operation)
        {
            var providerPermissions = await GetProviderPermissionsAsync(ukprn, operation);

            return await GetEmployerInfosAsync(providerPermissions);
        }

        public async Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProviderAsync(string accountHashedId, OperationType operation)
        {
            var providerPermissions = await GetProviderPermissionsAsync(accountHashedId, operation);
            
            return await GetEmployerInfosAsync(providerPermissions);
        }      

        public async Task<bool> HasProviderGotEmployersPermissionAsync(long ukprn, string accountHashedId, string accountLegalEntityPublicHashedId, OperationType operation)
        {
            var accountDetails = await _accountApiClient.GetAccount(accountHashedId);
            var permittedLegalEntities = await GetProviderPermissionsforEmployer(ukprn, accountDetails.PublicHashedAccountId, operation);

            if (permittedLegalEntities.Any() == false)
                return false;

            var accountId = await _employerAccountProvider.GetEmployerAccountPublicHashedIdAsync(permittedLegalEntities.First().AccountId);
            var allLegalEntities = (await _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(accountId)).ToList();

            var hasPermission = permittedLegalEntities.Join(allLegalEntities,
                    ple => ple.AccountLegalEntityPublicHashedId,
                    ale => ale.AccountLegalEntityPublicHashedId,
                    (ple, ale) => ale)
                .Any(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);

            return hasPermission;
        }

        private async Task<List<LegalEntityDto>> GetProviderPermissionsforEmployer(long ukprn, string accountPublicHashedId, OperationType operation)
        {
            var providerPermissions = await GetProviderPermissionsAsync(ukprn, operation);

            var permittedLegalEntities = providerPermissions.AccountProviderLegalEntities
                .Where(l => l.AccountPublicHashedId == accountPublicHashedId)
                .ToList();

            return permittedLegalEntities;
        }

        private async Task<ProviderPermissions> GetProviderPermissionsAsync(long ukprn, OperationType operation)
        {
            using (var httpClient = CreateHttpClient(_configuration))
            {
                var queryData = new { Ukprn = ukprn, Operation = operation.ToString() };
                var uri = new Uri(AddQueryString("accountproviderlegalentities", queryData), UriKind.RelativeOrAbsolute);

                try
                {
                    var response = await httpClient.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var providerPermissions = JsonConvert.DeserializeObject<ProviderPermissions>(content);
                        return providerPermissions;
                    }

                    _logger.LogError($"An invalid response received when trying to get provider relationships. Status:{response.StatusCode} Reason:{response.ReasonPhrase}");
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error trying to retrieve legal entities.", null);
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogError(ex, $"Couldn't deserialise {nameof(ProviderPermissions)}.", null);
                }

                return new ProviderPermissions { AccountProviderLegalEntities = Enumerable.Empty<LegalEntityDto>() };
            }
        }

        private async Task<ProviderPermissions> GetProviderPermissionsAsync(string accountHashedId, OperationType operation)
        {
            using (var httpClient = CreateHttpClient(_configuration))
            {
                var queryData = new { AccountHashedId = accountHashedId, Operation = operation.ToString() };
                var uri = new Uri(AddQueryString("accountproviderlegalentities", queryData), UriKind.RelativeOrAbsolute);

                try
                {
                    var response = await httpClient.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var providerPermissions = JsonConvert.DeserializeObject<ProviderPermissions>(content);
                        return providerPermissions;
                    }

                    _logger.LogError($"An invalid response received when trying to get provider relationships. Status:{response.StatusCode} Reason:{response.ReasonPhrase}");
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error trying to retrieve legal entities.", null);
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogError(ex, $"Couldn't deserialise {nameof(ProviderPermissions)}.", null);
                }

                return new ProviderPermissions { AccountProviderLegalEntities = Enumerable.Empty<LegalEntityDto>() };
            }
        }

        public async Task RevokeProviderPermissionToRecruitAsync(long ukprn, string accountLegalEntityPublicHashedId)
        {
            using (var httpClient = CreateHttpClient(_configuration))
            {
                var stringContent = GetStringContent(ukprn, accountLegalEntityPublicHashedId);
                var uri = new Uri(new Uri(_configuration.ApiBaseUrl), "permissions/revoke");

                var response = await httpClient.PostAsync(uri, stringContent);

                if(!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to revoke provider {ukprn} permission for account legal entity {accountLegalEntityPublicHashedId} response code: {response.StatusCode}");
                }
            }
        }


        private StringContent GetStringContent(long ukprn, string accountLegalEntityPublicHashedId)
        {
            var recruitOperationId = 1;
            var operationsToRevoke = new[]{recruitOperationId};
            var serializedData = JsonConvert.SerializeObject(new {ukprn , accountLegalEntityPublicHashedId, operationsToRevoke});
            return new StringContent(serializedData, Encoding.UTF8, "application/json");
        }

        private async Task<List<EmployerInfo>> GetEmployerInfosAsync(ProviderPermissions providerPermissions)
        {
            var employerInfos = new List<EmployerInfo>();

            var permittedEmployerAccounts = providerPermissions.AccountProviderLegalEntities.GroupBy(p => p.AccountId);

            foreach(var permittedEmployer in permittedEmployerAccounts)
            {
                var accountId = await _employerAccountProvider.GetEmployerAccountPublicHashedIdAsync(permittedEmployer.Key);

                var employerInfo = new EmployerInfo()
                {
                    EmployerAccountId = accountId,
                    Name = permittedEmployer.First().AccountName, //should be same in all the items hence read from first
                    LegalEntities = new List<LegalEntity>()
                };

                var legalEntityViewModels = await _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(accountId);

                foreach(var permittedLegalEntity in permittedEmployer)
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

        private static HttpClient CreateHttpClient(ProviderRelationshipApiConfiguration configuration)
        {

            var httpClient = new HttpClientBuilder()
                .WithDefaultHeaders()
                .WithBearerAuthorisationHeader(new AzureADBearerTokenGenerator(configuration))
                .Build();

            httpClient.BaseAddress = new Uri(configuration.ApiBaseUrl);

            return httpClient;
        }

        private string AddQueryString(string uri, object queryData)
        {
            var queryDataDictionary = queryData.GetType().GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(queryData)?.ToString() ?? string.Empty);
            return QueryHelpers.AddQueryString(uri, queryDataDictionary);
        }
    }
}