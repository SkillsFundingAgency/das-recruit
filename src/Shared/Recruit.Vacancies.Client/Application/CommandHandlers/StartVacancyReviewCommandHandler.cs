using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class StartVacancyReviewCommandHandler: IRequestHandler<StartVacancyReviewCommand>
    {
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly ITimeProvider _time;

        public StartVacancyReviewCommandHandler(IVacancyReviewRepository vacancyReviewRepository, ITimeProvider timeProvider)
        {
            _vacancyReviewRepository = vacancyReviewRepository;
            _time = timeProvider;
        }

        public async Task Handle(StartVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            var review = await _vacancyReviewRepository.GetAsync(message.ReviewId);

            if (review.Status != ReviewStatus.PendingReview)
            {
                throw new InvalidStateException($"Review is not in correct state to Start the review. Current State: {review.Status}");
            }

            review.Status = ReviewStatus.UnderReview;
            review.ReviewedByUserId = message.UserId;
            review.ReviewedDate = _time.Now;

            await _vacancyReviewRepository.UpdateAsync(review);
        }
    }
}
