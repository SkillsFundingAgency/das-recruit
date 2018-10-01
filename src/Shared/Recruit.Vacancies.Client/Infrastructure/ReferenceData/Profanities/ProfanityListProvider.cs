using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Profanities
{
    public class ProfanityListProvider : IProfanityListProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ILogger<ProfanityListProvider> _logger;

        public ProfanityListProvider(IReferenceDataReader referenceDataReader, ILogger<ProfanityListProvider> logger)
        {
            _referenceDataReader = referenceDataReader;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetProfanityListAsync()
        {
            _logger.LogInformation("Attempting to retrieve profanity list from reference data.");
            var result = await _referenceDataReader.GetReferenceData<ProfanityList>();
            if (result != null)
                return result.Profanities;
            _logger.LogWarning("Unable to retrieve reference data for profanity list.");
            return new List<string>();
        }
    }
}
