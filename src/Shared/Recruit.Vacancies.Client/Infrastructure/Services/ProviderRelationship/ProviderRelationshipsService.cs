using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
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
        private readonly IJobsVacancyClient _jobsVacancyClient;

        public ProviderRelationshipsService(IOptions<ProviderRelationshipApiConfiguration> configuration,
            ILogger<ProviderRelationshipApiConfiguration> logger,
            IJobsVacancyClient jobsVacancyClient)
        {
            _configuration = configuration.Value;
            _logger = logger;
            _jobsVacancyClient = jobsVacancyClient;
        }

        public async Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProviderAsync(long ukprn)
        {
            IEnumerable<EmployerInfo> employerInfos = new List<EmployerInfo>();
            var httpClient = CreateHttpClient(_configuration);
            var queryData = new { Ukprn = ukprn, Operation = "Recruitment" };            
            var uri = new Uri(AddQueryString("accountproviderlegalentities", queryData), UriKind.RelativeOrAbsolute);

            try
            {
                var response = await httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content  = await response.Content.ReadAsStringAsync();
                    var relations = Newtonsoft.Json.JsonConvert.DeserializeObject<ProviderPermissions>(content);
                    employerInfos =  MapEmployerInfo(relations);
                    await AppendAddressToLegalEntitiesAsync(employerInfos);
                }

                _logger.LogError("An invalid response received when trying to get provider relationships");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error trying to retrieve titles.", null);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, $"Couldn't deserialise {nameof(VacancyApiSearchResponse)}.", null);
            }

            return employerInfos;
        }

        private async Task AppendAddressToLegalEntitiesAsync(IEnumerable<EmployerInfo> employerInfos)
        {            
            foreach(var employerInfo in employerInfos)
            {
                var providerLegalEntities = employerInfo.LegalEntities;
                var employerLegalEntities = await _jobsVacancyClient.GetEmployerLegalEntitiesAsync(employerInfo.Id);

                foreach(var providerLegalEntity in providerLegalEntities)
                {
                    var source = employerLegalEntities.FirstOrDefault(e => e.LegalEntityId == providerLegalEntity.LegalEntityId);
                    if (source != null)
                    {
                        providerLegalEntity.Address = source.Address;
                    }                    
                }
            }
        }

        private IEnumerable<EmployerInfo> MapEmployerInfo(ProviderPermissions relations)
        {
            var result = new List<EmployerInfo>();

            return relations
                .AccountProviderLegalEntities
                .GroupBy(e => e.AccountPublicHashedId)
                .Select(e => new EmployerInfo() { 
                    Id = e.Key, 
                    Name = e.First().AccountName,
                    LegalEntities = e.Select(l => new LegalEntity{ LegalEntityId = l.AccountLegalEntityId, Name = l.AccountLegalEntityName }) 
                });
        }

        private HttpClient CreateHttpClient(ProviderRelationshipApiConfiguration configuration)
        {
            
            var httpClient = new HttpClientBuilder()
                .WithDefaultHeaders()
                .WithBearerAuthorisationHeader(new AzureADBearerTokenGenerator(configuration))
                .Build();
            
            httpClient.BaseAddress = new Uri(_configuration.ApiBaseUrl);

            return httpClient;
        }

        private string AddQueryString(string uri, object queryData)
        {
            var queryDataDictionary = queryData.GetType().GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(queryData)?.ToString() ?? string.Empty);
            return QueryHelpers.AddQueryString(uri, queryDataDictionary);
        }
    }

    class ProviderPermissions
    {
        public IEnumerable<LegalEntityDto> AccountProviderLegalEntities { get; set;}
    }
    class LegalEntityDto
    {
        public long AccountId { get; set; }
        public string AccountPublicHashedId { get; set; }
        public string AccountName { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public long AccountProviderId { get; set; }
    }
}