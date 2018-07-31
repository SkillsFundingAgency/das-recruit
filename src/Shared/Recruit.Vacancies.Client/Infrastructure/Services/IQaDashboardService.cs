using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public interface IQaDashboardService
    {
        Task RebuildQaDashboardAsync();
    }
}