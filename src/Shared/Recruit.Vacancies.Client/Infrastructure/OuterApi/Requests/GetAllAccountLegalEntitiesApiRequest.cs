using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetAllAccountLegalEntitiesApiRequest(GetAllAccountLegalEntitiesApiRequest.GetAllAccountLegalEntitiesApiRequestData Payload) : IPostApiRequest
{
    public string PostUrl => "accountlegalentities/getalllegalentities";
    public object Data { get; set; } = Payload;

    public record GetAllAccountLegalEntitiesApiRequestData
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortColumn { get; set; } = "Name";
        public bool IsAscending { get; set; } = false;
        public List<long> AccountIds { get; set; } = [];
    }
}