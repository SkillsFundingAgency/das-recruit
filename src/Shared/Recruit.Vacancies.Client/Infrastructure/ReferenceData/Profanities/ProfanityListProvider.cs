using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Profanities
{
    public class ProfanityListProvider : IProfanityListProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ILogger<ProfanityListProvider> _logger;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;

        public ProfanityListProvider(IReferenceDataReader referenceDataReader, ILogger<ProfanityListProvider> logger, ICache cache, ITimeProvider timeProvider)
        {
            _referenceDataReader = referenceDataReader;
            _logger = logger;
            _cache = cache;
            _timeProvider = timeProvider;
        }

        public async Task<IEnumerable<string>> GetProfanityListAsync()
        {
            return await _cache.CacheAsideAsync(CacheKeys.Profanities,
                _timeProvider.NextDay,
                async () =>
                {
                    _logger.LogInformation("Attempting to retrieve profanity list from reference data.");
                    var result = await _referenceDataReader.GetReferenceData<ProfanityList>();
                    if (result != null)
                        return result.Profanities;
                    _logger.LogWarning("Unable to retrieve reference data for profanity list.");
                    return new List<string>();
                });
        }
    }
}
