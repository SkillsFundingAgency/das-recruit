using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public class OuterApiGetProviderStatusClient : IGetProviderStatusClient
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<OuterApiGetAddressesClient> _logger;

        public OuterApiGetProviderStatusClient(IOuterApiClient outerApiClient, ILogger<OuterApiGetAddressesClient> logger)
        {
            _outerApiClient = outerApiClient;
            _logger = logger;
        }

        public async Task<ProviderAccountResponse> GetProviderStatus(long ukprn)
        {
            try
            {
                return await _outerApiClient.Get<ProviderAccountResponse>(new GetProviderStatusDetails(ukprn));
            }
            catch (Exception e)
            {
                string message = $"Get Provider Status failed for ukprn number: {ukprn}.";
                _logger.LogDebug(message);
                throw new Exception(message, e);
            }
        }
    }
}
