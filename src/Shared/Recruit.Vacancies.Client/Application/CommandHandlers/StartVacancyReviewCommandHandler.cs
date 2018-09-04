using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class StartVacancyReviewCommandHandler: IRequestHandler<StartVacancyReviewCommand>
    {
        private readonly ILogger<StartVacancyReviewCommandHandler> _logger;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly ITimeProvider _time;

        public StartVacancyReviewCommandHandler(
            ILogger<StartVacancyReviewCommandHandler> logger,
            IVacancyReviewRepository vacancyReviewRepository, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _vacancyReviewRepository = vacancyReviewRepository;
            _time = timeProvider;
        }

        public async Task Handle(StartVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting vacancy review {reviewId}.", message.ReviewId);

            var review = await _vacancyReviewRepository.GetAsync(message.ReviewId);

            if (!review.CanAssign)
            {
                _logger.LogWarning($"Unable to assign review {{reviewId}} for vacancy {{vacancyReference}} due to review having a status of {review.Status}.", message.ReviewId, review.VacancyReference);
            }

            review.Status = ReviewStatus.UnderReview;
            review.ReviewedByUser = message.User;
            review.ReviewedDate = _time.Now;

            await _vacancyReviewRepository.UpdateAsync(review);
        }
    }
}
