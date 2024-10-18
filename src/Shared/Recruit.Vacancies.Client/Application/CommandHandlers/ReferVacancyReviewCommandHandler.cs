using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ReferVacancyReviewCommandHandler : IRequestHandler<ReferVacancyReviewCommand, Unit>
    {
        private readonly ILogger<ReferVacancyReviewCommandHandler> _logger;
        private readonly IVacancyReviewRepository _reviewRepository;
        private readonly IMessaging _messaging;
        private readonly AbstractValidator<VacancyReview> _vacancyReviewValidator;
        private readonly ITimeProvider _timeProvider;

        public ReferVacancyReviewCommandHandler(
            ILogger<ReferVacancyReviewCommandHandler> logger,
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

        public async Task<Unit> Handle(ReferVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Referring vacancy review {reviewId}.", message.ReviewId);

            var review = await _reviewRepository.GetAsync(message.ReviewId);

            if (!review.CanRefer)
            {
                _logger.LogWarning($"Unable to refer review {{reviewId}} for vacancy {{vacancyReference}} due to review having a status of {review.Status}.", message.ReviewId, review.VacancyReference);
                return Unit.Value;
            }

            review.ManualOutcome = ManualQaOutcome.Referred;
            review.Status = ReviewStatus.Closed;
            review.ClosedDate = _timeProvider.Now;
            review.ManualQaComment = message.ManualQaComment;
            review.ManualQaFieldIndicators = message.ManualQaFieldIndicators;

            foreach (var automatedQaOutcomeIndicator in review.AutomatedQaOutcomeIndicators)
            {
                automatedQaOutcomeIndicator.IsReferred = message.SelectedAutomatedQaRuleOutcomeIds
                    .Contains(automatedQaOutcomeIndicator.RuleOutcomeId);
            }

            var fields = new List<string>();
            var referredOutcomes = review.AutomatedQaOutcomeIndicators
                .Where(i => !i.IsReferred)
                .Select(i => i.RuleOutcomeId)
                .ToList();
            foreach (var ruleOutcome in review.AutomatedQaOutcome.RuleOutcomes)
            {
                fields.AddRange(ruleOutcome.Details
                    .Where(d => referredOutcomes.Contains(d.Id))
                    .Select(d => d.Target).ToList());
            }
            
                
            review.DismissedAutomatedQaOutcomeIndicators = fields.Distinct().ToList();

            Validate(review);

            await _reviewRepository.UpdateAsync(review);

            await _messaging.PublishEvent(new VacancyReviewReferredEvent
            {
                VacancyReference = review.VacancyReference,
                ReviewId = review.Id
            });
            return Unit.Value;
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
