using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship
{
    public class ProviderRelationshipsService : IProviderRelationshipsService
    {
        private readonly ProviderRelationshipApiConfiguration _configuration;
        private readonly ILogger<ProviderRelationshipApiConfiguration> _logger;
        private readonly IEmployerAccountProvider _employerAccountProvider;  

        public ProviderRelationshipsService(IOptions<ProviderRelationshipApiConfiguration> configuration,
            ILogger<ProviderRelationshipApiConfiguration> logger,
            IEmployerAccountProvider employerAccountProvider)
        {
            _configuration = configuration.Value;
            _logger = logger;
            _employerAccountProvider = employerAccountProvider;
        }

        public async Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProviderAsync(long ukprn)
        {
            ProviderPermissions providerPermissions = null;
            var httpClient = CreateHttpClient(_configuration);
            var queryData = new { Ukprn = ukprn, Operation = "Recruitment" };            
            var uri = new Uri(AddQueryString("accountproviderlegalentities", queryData), UriKind.RelativeOrAbsolute);

            try
            {
                var response = await httpClient.GetAsync(uri);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("An invalid response received when trying to get provider relationships");
                    return new EmployerInfo[]{};
                }                
                var content  = await response.Content.ReadAsStringAsync();
                providerPermissions = JsonConvert.DeserializeObject<ProviderPermissions>(content);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error trying to retrieve legal entities.", null);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, $"Couldn't deserialise {nameof(ProviderPermissions)}.", null);
            }

            return await GetEmployerInfosAsync(providerPermissions);
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
                    Name = permittedEmployer.First().AccountName, //should be same in all the items hense read from first
                    LegalEntities = new List<LegalEntity>() 
                };
                
                var legalEntityViewModels = await _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(accountId);

                foreach(var permittedLegalEntity in permittedEmployer)
                {
                    var matchingLegalEntity = legalEntityViewModels.FirstOrDefault(e => e.AccountLegalEntityPublicHashedId == permittedLegalEntity.AccountLegalEntityPublicHashedId);
                    if (matchingLegalEntity != null)
                    {
                        var legalEntity = LegalEntityMapper.MapFromAccountApiLegalEntity(matchingLegalEntity);
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