using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetAccountLegalEntitiesResponse
{
    [JsonProperty("accountLegalEntities")]
    public List<AccountLegalEntity> AccountLegalEntities { get; set; }
}
    
public class AccountLegalEntity
{
    [JsonProperty("hasLegalAgreement")]
    public bool HasLegalAgreement { get; set; }

    [JsonProperty("accountLegalEntityPublicHashedId")]
    public string AccountLegalEntityPublicHashedId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("accountLegalEntityId")]
    public long AccountLegalEntityId { get; set; }
    [JsonProperty("dasAccountId")]
    public string DasAccountId { get; set; }
    [JsonProperty("legalEntityId")]
    public long LegalEntityId { get; set; }
}