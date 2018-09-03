using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public interface ILiveVacancyProjectionService
    {
        Task ReGenerateLiveVacanciesAsync();
    }
}
