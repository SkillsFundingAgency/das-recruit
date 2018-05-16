using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface INotifyVacancyReviewUpdates
    {
        Task VacancyReviewCreated(long vacancyReference);
        Task VacancyReviewReferred(long vacancyReference);
        Task VacancyReviewApproved(long vacancyReference);
    }
}