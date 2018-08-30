using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammeProvider : IApprenticeshipProgrammeProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;

        public ApprenticeshipProgrammeProvider(IReferenceDataReader queryStoreReader)
        {
            _referenceDataReader = queryStoreReader;
        }

        public async Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId)
        {
            var apprenticeships = await GetApprenticeshipProgrammesAsync(true);

            return apprenticeships?.SingleOrDefault(x => x.Id == programmeId);
        }

        public async Task<IEnumerable<IApprenticeshipProgramme>> GetApprenticeshipProgrammesAsync(bool includeExpired = false)
        {
            var queryItem = await _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>();

            if (includeExpired)
                return queryItem.Data;
            else
                return queryItem.Data.Where(x => x.IsActive);
        }
    }
}