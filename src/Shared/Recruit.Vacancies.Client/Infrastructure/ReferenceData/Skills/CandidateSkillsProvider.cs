using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Skills
{
    public class CandidateSkillsProvider : ICandidateSkillsProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ILogger<CandidateSkillsProvider> _logger;

        public CandidateSkillsProvider(IReferenceDataReader referenceDataReader, ILogger<CandidateSkillsProvider> logger)
        {
            _referenceDataReader = referenceDataReader;
            _logger = logger;
        }
        public async Task<List<string>> GetCandidateSkillsAsync()
        {
            var result = await _referenceDataReader.GetReferenceData<CandidateSkills>();
            if (result != null)
                return result.Skills;
            _logger.LogWarning("Unable to retrieve reference data for qualifications");
            return new List<string>();
        }
    }
}
