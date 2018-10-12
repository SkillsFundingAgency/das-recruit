using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public interface IPublishedVacancyProjectionService
    {
        Task ReGeneratePublishedVacanciesAsync();
    }
}
