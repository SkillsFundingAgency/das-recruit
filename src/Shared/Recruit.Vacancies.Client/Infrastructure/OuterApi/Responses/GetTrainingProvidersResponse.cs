using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public class GetTrainingProvidersResponse
    {
        [JsonProperty("providers")]
        public IEnumerable<GetTrainingProviderResponseItem> Providers { get; set; }
    }

    public class GetTrainingProviderResponseItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ukprn")]
        public long Ukprn { get; set; }
        [JsonProperty("phone")]
        public string Phone { get; set; }
        [JsonProperty("providerTypeId")]
        public int ProviderTypeId { get; set; }


        [JsonProperty("address")]
        public ProviderAddress Address { get; set; }
    }
}