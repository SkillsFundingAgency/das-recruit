using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class GetAddressesRequest : IGetApiRequest
{
    private readonly string _query;

    public GetAddressesRequest(string query)
    {
        _query = query;
    }

    string IGetApiRequest.GetUrl => $"locations?query={_query}";
      
}