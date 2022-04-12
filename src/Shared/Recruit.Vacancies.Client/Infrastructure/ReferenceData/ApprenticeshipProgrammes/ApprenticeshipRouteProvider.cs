using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipRouteProvider : IApprenticeshipRouteProvider
    {
        private readonly IReferenceDataReader _queryStoreReader;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;

        public ApprenticeshipRouteProvider(IReferenceDataReader queryStoreReader, ICache cache, ITimeProvider timeProvider)
        {
            _queryStoreReader = queryStoreReader;
            _cache = cache;
            _timeProvider = timeProvider;
        }
        public async Task<IApprenticeshipRoute> GetApprenticeshipRouteAsync(int routeId)
        {
            var apprenticeships = await GetApprenticeshipRoutesAsync();

            return apprenticeships?.SingleOrDefault(x => x.Id == routeId);
        }

        public async Task<IEnumerable<IApprenticeshipRoute>> GetApprenticeshipRoutesAsync()
        {
            var queryItem = await GetApprenticeshipRoutes();
            return queryItem.Data;

        }
        private Task<ApprenticeshipRoutes> GetApprenticeshipRoutes()
        {
            return _cache.CacheAsideAsync(CacheKeys.ApprenticeshipRoutes,
                _timeProvider.NextDay6am,
                () => _queryStoreReader.GetReferenceData<ApprenticeshipRoutes>());
        }
    }
}