using System;
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

        private DateTime CacheAbsoluteExpiryTime => DateTime.UtcNow.Date.AddDays(1);

        public QualificationsProvider(IReferenceDataReader referenceDataReader, ICache cache)
        {
            _referenceDataReader = referenceDataReader;
            _cache = cache;
        }

        public async Task<IList<string>> GetQualificationsAsync()
        {
            return await _cache.CacheAsideAsync(CacheKeys.Qualifications,
                CacheAbsoluteExpiryTime,
                async () =>
                {
                    var data = await _referenceDataReader.GetReferenceData<Qualifications>();
                    return data.QualificationTypes;
                });
        }
    }
}