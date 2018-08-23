using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public interface IEmployerDashboardService
    {
        Task ReBuildDashboardAsync(string employerAccountId);
    }
}
