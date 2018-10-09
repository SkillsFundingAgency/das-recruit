using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public interface IEmployerDashboardProjectionService
    {
        Task ReBuildAllDashboardsAsync();
        Task ReBuildDashboardAsync(string employerAccountId);
    }
}
