using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface INotifyVacancyReviewUpdates
    {
        Task NewVacancyReview(long vacancyReference);
    }
}