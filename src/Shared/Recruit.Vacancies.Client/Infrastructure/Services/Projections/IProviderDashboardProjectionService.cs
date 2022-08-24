using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public interface IProviderDashboardProjectionService
    {
        Task ReBuildAllDashboardsAsync();
        Task ReBuildDashboardAsync(long ukprn, VacancyType? vacancyType = null);
    }
}
