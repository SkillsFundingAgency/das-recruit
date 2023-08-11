using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public class GetTrainingProvidersResponse
    {
        [JsonProperty("registeredProviders")]
        public IEnumerable<GetTrainingProviderResponseItem> RegisteredProviders { get; set; }
    }

    public class GetTrainingProviderResponseItem
    {
        public string Name { get; set; }
        public long Ukprn { get; set; }
        public int ProviderTypeId { get; set; }

        [JsonProperty(nameof(Address))]
        public ProviderAddress Address { get; set; }
    }
}