using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public class OuterApiGetAddressesClient : IGetAddressesClient
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<OuterApiGetAddressesClient> _logger;

        public OuterApiGetAddressesClient(IOuterApiClient outerApiClient, ILogger<OuterApiGetAddressesClient> logger)
        {
            _outerApiClient = outerApiClient;
            _logger = logger;
        }

        public async Task<GetAddressesListResponse> GetAddresses(string searchTerm)
        {
            try
            {
               return await _outerApiClient.Get<GetAddressesListResponse>(new GetAddressesRequest(searchTerm));
            }
            catch(Exception e)
            {
                string message = $"Get addresses failed for search term: {searchTerm}.";
                _logger.LogDebug(message);
                throw new Exception(message, e);
            }
        }
    }
}
