using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications
{
    public class QualificationsProvider : IQualificationsProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;

        public QualificationsProvider(IReferenceDataReader referenceDataReader, ICache cache, ITimeProvider timeProvider)
        {
            _referenceDataReader = referenceDataReader;
            _cache = cache;
            _timeProvider = timeProvider;
        }

        public async Task<IList<string>> GetQualificationsAsync()
        {
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