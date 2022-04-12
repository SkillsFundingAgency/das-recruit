using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface IApprenticeshipRouteProvider
    {
        Task<IApprenticeshipRoute> GetApprenticeshipRouteAsync(int routeId);
        Task<IEnumerable<IApprenticeshipRoute>> GetApprenticeshipProgrammesAsync();
    }
}