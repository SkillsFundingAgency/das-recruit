using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public interface IProviderDashboardProjectionService
    {
        Task ReBuildAllDashboardsAsync();
        Task ReBuildDashboardAsync(long ukprn);
    }
}
