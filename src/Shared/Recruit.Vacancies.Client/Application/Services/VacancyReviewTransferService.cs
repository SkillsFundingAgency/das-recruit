using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class VacancyReviewTransferService : IVacancyReviewTransferService
    {
        private readonly ILogger<VacancyReviewTransferService> _logger;
        private readonly IVacancyReviewQuery _vacancyReviewQuery;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly ITimeProvider _timeProvider;

        public VacancyReviewTransferService(ILogger<VacancyReviewTransferService> logger, IVacancyReviewQuery vacancyReviewQuery,
                                            IVacancyReviewRepository vacancyReviewRepository, ITimeProvider timeProvider)
        {
            _logger = logger;
            _vacancyReviewQuery = vacancyReviewQuery;
            _vacancyReviewRepository = vacancyReviewRepository;
            _timeProvider = timeProvider;
        }

        public async Task CloseVacancyReview(long vacancyReference, TransferReason transferReason)
        {
            var review = await _vacancyReviewQuery.GetLatestReviewByReferenceAsync(vacancyReference);

            if (review.Status != ReviewStatus.Closed)
            {
                if (review.Status == ReviewStatus.New || review.Status == ReviewStatus.PendingReview)
                {
                    review.ManualOutcome = review.VacancySnapshot.OwnerType == OwnerType.Provider && transferReason == TransferReason.BlockedByQa
                                            ? ManualQaOutcome.Withdrawn
                                            : ManualQaOutcome.Transferred;
                }
                else if (review.Status == ReviewStatus.UnderReview)
                {
                    _logger.LogWarning($"Latest review for vacancy {review.VacancyReference} that has been transferred is currently being reviewed.");
                }

                SetVacancyReviewClosed(review);
                await _vacancyReviewRepository.UpdateAsync(review);
            }
        }

        private void SetVacancyReviewClosed(VacancyReview review)
        {
            review.Status = ReviewStatus.Closed;
            review.ClosedDate = _timeProvider.Now;
        }
    }
}