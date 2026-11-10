using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public record GetAllAccountLegalEntitiesApiResponse
{
    [JsonProperty("pageInfo")]
    public PaginationInfo PageInfo { get; set; }

    [JsonProperty("legalEntities")]
    public List<LegalEntity> LegalEntities { get; set; }

    public class LegalEntity
    {
        [JsonProperty("agreements")]
        public List<Agreement> Agreements { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("accountLegalEntityPublicHashedId")]
        public string AccountLegalEntityPublicHashedId { get; set; }

        [JsonProperty("legalEntityId")]
        public int LegalEntityId { get; set; }

        [JsonProperty("accountLegalEntityId")]
        public int AccountLegalEntityId { get; set; }

        [JsonProperty("dasAccountId")]
        public string DasAccountId { get; set; }
        [JsonProperty("accountName")]
        public string AccountName { get; set; }
    }

    public class Agreement
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("agreementType")]
        public int AgreementType { get; set; }

        [JsonProperty("templateVersionNumber")]
        public int TemplateVersionNumber { get; set; }
    }

    public class PaginationInfo
    {
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("hasPreviousPage")]
        public bool HasPreviousPage { get; set; }

        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }
    }
}