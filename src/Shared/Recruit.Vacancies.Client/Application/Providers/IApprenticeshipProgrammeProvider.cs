using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface IApprenticeshipProgrammeProvider
    {
        Task<IEnumerable<IApprenticeshipProgramme>> GetApprenticeshipProgrammesAsync(bool includeExpired = false, int? ukprn = null, bool includePlaceholderProgramme = false);

        Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId, int? ukprn = null, bool includePlaceholderProgramme = false);

        Task<ApprenticeshipStandard> GetApprenticeshipStandardVacancyPreviewData(int programmedId);
    }
}