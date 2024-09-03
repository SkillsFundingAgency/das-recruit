using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Providers;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications
{
    public class QualificationsProvider : IQualificationsProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;
        private readonly bool _faaV2FeatureEnabled;

        public QualificationsProvider(IReferenceDataReader referenceDataReader, ICache cache, ITimeProvider timeProvider, IFeature feature)
        {
            _referenceDataReader = referenceDataReader;
            _cache = cache;
            _timeProvider = timeProvider;
            _faaV2FeatureEnabled = feature.IsFeatureEnabled("FaaV2Improvements");
        }

        public async Task<IList<string>> GetQualificationsAsync()
        {
            if (_faaV2FeatureEnabled)
            {
                return new List<string>
                {
                    "GCSE",
                    "A Level",
                    "T Level",
                    "BTEC",
                    "Degree",
                    "Other"
                };
            }
            return await _cache.CacheAsideAsync(CacheKeys.Qualifications,
                _timeProvider.NextDay,
                async () =>
                {
                    var data = await _referenceDataReader.GetReferenceData<Qualifications>();
                    return data.QualificationTypes;
                });
        }
    }
}