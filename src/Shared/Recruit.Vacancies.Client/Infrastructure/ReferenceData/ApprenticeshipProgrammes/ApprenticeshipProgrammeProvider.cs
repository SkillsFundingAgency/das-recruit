using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammeProvider : IApprenticeshipProgrammeProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ICache _cache;

        private DateTime CacheAbsoluteExpiryTime => DateTime.UtcNow.Date.AddDays(1);

        public ApprenticeshipProgrammeProvider(IReferenceDataReader queryStoreReader, ICache cache)
        {
            _referenceDataReader = queryStoreReader;
            _cache = cache;
        }

        public async Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId)
        {
            var apprenticeships = await GetApprenticeshipProgrammesAsync(true);

            return apprenticeships?.SingleOrDefault(x => x.Id == programmeId);
        }

        public async Task<IEnumerable<IApprenticeshipProgramme>> GetApprenticeshipProgrammesAsync(bool includeExpired = false)
        {
            var queryItem = await GetApprenticeshipProgrammesAsync();

            return includeExpired ? 
                queryItem.Data : 
                queryItem.Data.Where(x => x.IsActive);
        }

        private Task<ApprenticeshipProgrammes> GetApprenticeshipProgrammesAsync()
        {
            return _cache.CacheAsideAsync(CacheKeys.ApprenticeshipProgrammes,
                CacheAbsoluteExpiryTime,
                () => _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>());
        }
    }
}