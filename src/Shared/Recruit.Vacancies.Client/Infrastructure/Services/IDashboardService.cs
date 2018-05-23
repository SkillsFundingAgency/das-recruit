using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public interface IDashboardService
    {
        Task ReBuildDashboardAsync(string employerAccountId);
    }
}
