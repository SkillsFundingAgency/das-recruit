using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface INotifyVacancyReviewUpdates
    {
        Task VacancyReviewCreated(VacancyReview vacancyReview);
        Task VacancyReviewReferred(VacancyReview vacancyReview);
        Task VacancyReviewApproved(VacancyReview vacancyReview);
    }
}