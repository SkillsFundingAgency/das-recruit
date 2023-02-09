using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BannedPhrases
{
    public class BannedPhrasesProvider : IBannedPhrasesProvider
    {
        private readonly ILogger<BannedPhrasesProvider> _logger;
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;

        public BannedPhrasesProvider(ILogger<BannedPhrasesProvider> logger, IReferenceDataReader referenceDataReader, ICache cache, ITimeProvider timeProvider)
        {
            _logger = logger;
            _referenceDataReader = referenceDataReader;
            _cache = cache;
            _timeProvider = timeProvider;
        }
        public async Task<IEnumerable<string>> GetBannedPhrasesAsync()
        {
            return await _cache.CacheAsideAsync(CacheKeys.BannedPhrases,
                _timeProvider.NextDay,
                async () =>
                {
                    _logger.LogInformation("Attempting to retrieve banned phrases list from reference data.");
                    var result = await _referenceDataReader.GetReferenceData<BannedPhraseList>();
                    if (result != null)
                        return result.BannedPhrases;
                    _logger.LogWarning("Unable to retrieve reference data for banned phrases list.");
                    return new List<string>();
                });
        }
    }
}