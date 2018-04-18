using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.Models;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IApprenticeshipProgrammeProvider
    {
        Task<IEnumerable<IApprenticeshipProgramme>> GetApprenticeshipProgrammesAsync(bool includeExpired = false);

        Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId);
    }
}