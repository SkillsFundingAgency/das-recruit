using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public class OuterApiGetProviderStatusClient(
        IOuterApiClient outerApiClient,
        ICache cache,
        ITimeProvider timeProvider,
        ILogger<OuterApiGetProviderStatusClient> logger)
        : IGetProviderStatusClient
    {
        public async Task<ProviderAccountResponse> GetProviderStatus(long ukprn)
        {
            if (ukprn == EsfaTestTrainingProvider.Ukprn)
            {
                return new ProviderAccountResponse
                {
                    CanAccessService = true
                };
            }
            try
            {
                return await cache.CacheAsideAsync($"{CacheKeys.ProviderAccountsPermissions}_{ukprn}",
                    timeProvider.NextDay,
                    async () =>
                    {
                        var retryPolicy = PollyRetryPolicy.GetPolicy();

                        var response = await retryPolicy.Execute(_ => 
                                outerApiClient.Get<ProviderAccountResponse>(new GetProviderStatusDetails(ukprn)),
                            new Dictionary<string, object>
                            {
                                {
                                    "apiCall", "ProviderAccounts"
                                }
                            });

                        return response;
                    });
            }
            catch (Exception e)
            {
                logger.LogDebug("Get Provider Status failed for ukprn number: {Ukprn}.", ukprn);
                throw new Exception($"Get Provider Status failed for ukprn number: {ukprn}.", e);
            }
        }
    }
}
