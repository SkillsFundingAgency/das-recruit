using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ReferVacancyReviewCommandHandler : IRequestHandler<ReferVacancyReviewCommand>
    {
        private readonly ILogger<ReferVacancyCommandHandler> _logger;
        private readonly IVacancyReviewRepository _reviewRepository;
        private readonly IMessaging _messaging;

        public ReferVacancyReviewCommandHandler(
            ILogger<ReferVacancyCommandHandler> logger,
            IVacancyReviewRepository reviewRepository, 
            IMessaging messaging)
        {
            _logger = logger;
            _reviewRepository = reviewRepository;
            _messaging = messaging;
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

            await _reviewRepository.UpdateAsync(review);
            
            await _messaging.PublishEvent(new VacancyReviewReferredEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                VacancyReference = review.VacancyReference,
                ReviewId = review.Id
            });
        }
    }
}
