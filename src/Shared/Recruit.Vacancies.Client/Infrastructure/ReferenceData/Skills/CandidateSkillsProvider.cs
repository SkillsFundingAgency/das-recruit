using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Skills
{
    public class CandidateSkillsProvider : ICandidateSkillsProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ILogger<CandidateSkillsProvider> _logger;
        private readonly ICache _cache;

        private DateTime CacheAbsoluteExpiryTime => DateTime.UtcNow.Date.AddDays(1);

        public CandidateSkillsProvider(IReferenceDataReader referenceDataReader, ILogger<CandidateSkillsProvider> logger, ICache cache)
        {
            _referenceDataReader = referenceDataReader;
            _logger = logger;
            _cache = cache;
        }
        public async Task<List<string>> GetCandidateSkillsAsync()
        {
            return await _cache.CacheAsideAsync(CacheKeys.Skills,
                CacheAbsoluteExpiryTime,
                async () =>
                {
                    _logger.LogInformation("Attempting to retrieve candidate skills from reference data.");
                    var result = await _referenceDataReader.GetReferenceData<CandidateSkills>();
                    if (result != null)
                        return result.Skills;
                    _logger.LogWarning("Unable to retrieve reference data for candidate skills.");
                    return new List<string>();
                });
        }
    }
}
