using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BannedPhrases;

public class BannedPhrasesProvider(
    ILogger<BannedPhrasesProvider> logger,
    ICache cache,
    ITimeProvider timeProvider,
    IOuterApiClient outerApiClient)
    : IBannedPhrasesProvider
{
    public async Task<IEnumerable<string>> GetBannedPhrasesAsync() =>
        await cache.CacheAsideAsync(CacheKeys.BannedPhrases,
            timeProvider.NextDay,
            async () =>
            {
                logger.LogInformation("Attempting to retrieve banned phrases list from reference data.");

                var results = await outerApiClient.Get<List<string>>(new GetBannedPhrasesRequest());
                if (results is { Count: > 0 })
                {
                    return results;
                }
                
                logger.LogWarning("Unable to retrieve reference data for banned phrases list.");
                return [];
            });
}