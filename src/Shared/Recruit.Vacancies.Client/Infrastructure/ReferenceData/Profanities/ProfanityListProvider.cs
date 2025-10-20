using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Profanities;

public class ProfanityListProvider(
    ILogger<ProfanityListProvider> logger,
    ICache cache,
    ITimeProvider timeProvider,
    IOuterApiClient outerApiClient)
    : IProfanityListProvider
{
    public async Task<IEnumerable<string>> GetProfanityListAsync() =>
        await cache.CacheAsideAsync(CacheKeys.Profanities,
            timeProvider.NextDay,
            async () =>
            {
                var results = await outerApiClient.Get<List<string>>(new GetProfanitiesRequest());
                if (results is { Count: > 0 })
                {
                    return results;
                }
                
                logger.LogWarning("Unable to retrieve reference data for profanity list.");
                return [];
            });
}