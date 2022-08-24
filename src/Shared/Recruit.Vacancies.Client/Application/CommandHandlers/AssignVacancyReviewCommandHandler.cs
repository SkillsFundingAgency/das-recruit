using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class AssignVacanyReviewCommandHandler: IRequestHandler<AssignVacancyReviewCommand, Unit>
    {
        private readonly ILogger<AssignVacancyReviewCommand> _logger;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly ITimeProvider _time;
        private readonly INextVacancyReviewService _nextVacancyReviewService;

        public AssignVacanyReviewCommandHandler(
            ILogger<AssignVacancyReviewCommand> logger,
            IVacancyReviewRepository vacancyReviewRepository, 
            ITimeProvider timeProvider,
            INextVacancyReviewService nextVacancyReviewService)
        {
            _logger = logger;
            _vacancyReviewRepository = vacancyReviewRepository;
            _time = timeProvider;
            _nextVacancyReviewService = nextVacancyReviewService;
        }

        public async Task<Unit> Handle(AssignVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            VacancyReview review;

            if (message.ReviewId.HasValue)
            {
                _logger.LogInformation("Starting assignment of review for user {userId} for review {reviewId}", message.User.UserId, message.ReviewId);
                review = await GetVacancyReviewAsync(message.ReviewId.Value);
            }
            else
            {
                _logger.LogInformation("Starting assignment of next review for user {userId}.", message.User.UserId);
                review = await GetNextVacancyReviewForUser(message.User.UserId);
            }

            if (review == null)
                return Unit.Value;

            review.Status = ReviewStatus.UnderReview;
            review.ReviewedByUser = message.User;
            review.ReviewedDate = _time.Now;

            await _vacancyReviewRepository.UpdateAsync(review);
            return Unit.Value;
        }

        private async Task<VacancyReview> GetVacancyReviewAsync(Guid reviewId)
        {
            var review = await _vacancyReviewRepository.GetAsync(reviewId);

            if (_nextVacancyReviewService.VacancyReviewCanBeAssigned(review.Status, review.ReviewedDate))
                return review;

            _logger.LogWarning($"Unable to assign review {{reviewId}} for vacancy {{vacancyReference}} due to review having a status of {review.Status}.", review.Id, review.VacancyReference);
            return null;
        }

        private async Task<VacancyReview> GetNextVacancyReviewForUser(string userId)
        {
            var review = await _nextVacancyReviewService.GetNextVacancyReviewAsync(userId);

            if (review == null)
            {
                _logger.LogInformation("No reviews to assign to user {userId}.", userId);
            }

            return review;
        }
    }
}
