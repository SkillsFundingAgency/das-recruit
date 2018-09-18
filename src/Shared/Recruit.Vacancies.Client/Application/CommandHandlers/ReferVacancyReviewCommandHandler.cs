using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ReferVacancyReviewCommandHandler : IRequestHandler<ReferVacancyReviewCommand>
    {
        private readonly ILogger<ReferVacancyCommandHandler> _logger;
        private readonly IVacancyReviewRepository _reviewRepository;
        private readonly IMessaging _messaging;
        private readonly AbstractValidator<VacancyReview> _vacancyReviewValidator;
        private readonly ITimeProvider _timeProvider;

        public ReferVacancyReviewCommandHandler(
            ILogger<ReferVacancyCommandHandler> logger,
            IVacancyReviewRepository reviewRepository, 
            IMessaging messaging,
            AbstractValidator<VacancyReview> vacancyReviewValidator,
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _reviewRepository = reviewRepository;
            _messaging = messaging;
            _timeProvider = timeProvider;
            _vacancyReviewValidator = vacancyReviewValidator;
        }

        public async Task Handle(ReferVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Referring vacancy review {reviewId}.", message.ReviewId);

            var review = await _reviewRepository.GetAsync(message.ReviewId);

            if (!review.CanRefer)
            {
                _logger.LogWarning($"Unable to refer review {{reviewId}} for vacancy {{vacancyReference}} due to review having a status of {review.Status}.", message.ReviewId, review.VacancyReference);
                return;
            }

            review.ManualOutcome = ManualQaOutcome.Referred;
            review.Status = ReviewStatus.Closed;
            review.ClosedDate = _timeProvider.Now;
            review.ManualQaComment = message.ManualQaComment;
            review.ManualQaFieldIndicators = message.ManualQaFieldIndicators;

            Validate(review);

            await _reviewRepository.UpdateAsync(review);
            
            await _messaging.PublishEvent(new VacancyReviewReferredEvent
            {
                VacancyReference = review.VacancyReference,
                ReviewId = review.Id
            });
        }

        private void Validate(VacancyReview review)
        {
            var validationResult = _vacancyReviewValidator.Validate(review);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}
