using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class GetAddressesRequest(string query) : IGetApiRequest
{
    string IGetApiRequest.GetUrl => $"locations?query={query}";
      
}