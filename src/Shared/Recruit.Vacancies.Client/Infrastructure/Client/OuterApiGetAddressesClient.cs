using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

public class OuterApiGetAddressesClient(IOuterApiClient outerApiClient, ILogger<OuterApiGetAddressesClient> logger)
    : IGetAddressesClient
{
    public async Task<GetAddressesListResponse> GetAddresses(string searchTerm)
    {
        try
        {
            return await outerApiClient.Get<GetAddressesListResponse>(new GetAddressesRequest(searchTerm));
        }
        catch(Exception e)
        {
            string message = $"Get addresses failed for search term: {searchTerm}.";
            logger.LogDebug(message);
            throw new Exception(message, e);
        }
    }
}