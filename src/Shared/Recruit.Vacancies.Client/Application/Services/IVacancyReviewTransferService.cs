using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IVacancyReviewTransferService
    {
        Task CloseVacancyReview(long vacancyReference, TransferReason transferReason);
    }
}