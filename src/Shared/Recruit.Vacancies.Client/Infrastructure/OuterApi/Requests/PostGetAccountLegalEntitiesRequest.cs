using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class PostGetAccountLegalEntitiesRequest(List<long> accountIds) : IPostApiRequest
{
    public string PostUrl => "employeraccounts/legalentities";
    public object Data { get; set; } = accountIds;
}