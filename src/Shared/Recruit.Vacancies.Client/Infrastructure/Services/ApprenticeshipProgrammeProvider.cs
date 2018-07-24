using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class ApprenticeshipProgrammeProvider : IApprenticeshipProgrammeProvider
    {
        private readonly IQueryStoreReader _queryStoreReader;

        public ApprenticeshipProgrammeProvider(IQueryStoreReader queryStoreReader)
        {
            _queryStoreReader = queryStoreReader;
        }

        public async Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId)
        {
            var apprenticeships = await GetApprenticeshipProgrammesAsync(true);

            return apprenticeships?.SingleOrDefault(x => x.Id == programmeId);
        }

        public async Task<IEnumerable<IApprenticeshipProgramme>> GetApprenticeshipProgrammesAsync(bool includeExpired = false)
        {
            var queryItem = await _queryStoreReader.GetApprenticeshipProgrammesAsync();

            if (includeExpired)
                return queryItem.Programmes;
            else
                return queryItem.Programmes.Where(x => x.IsActive);
        }
    }
}