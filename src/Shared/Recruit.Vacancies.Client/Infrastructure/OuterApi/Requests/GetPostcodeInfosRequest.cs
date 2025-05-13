using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetBulkPostcodeDataRequest(List<string> Postcodes): IPostApiRequest
{
    public string PostUrl => $"postcodes/bulk/";
    public object Data { get; set; } = Postcodes;
}