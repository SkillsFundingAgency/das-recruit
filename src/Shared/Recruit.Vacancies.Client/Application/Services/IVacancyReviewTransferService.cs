using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IVacancyReviewTransferService
    {
        Task CloseVacancyReview(long vacancyReference, bool hasProviderBeenBlocked);
    }
}