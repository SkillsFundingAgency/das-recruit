using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApproveReferredVacancyReviewCommandHandler: IRequestHandler<ApproveReferredVacancyReviewCommand>
    {
        private readonly ILogger<ApproveReferredVacancyReviewCommandHandler> _logger;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMessaging _messaging;

        public ApproveReferredVacancyReviewCommandHandler(
                            ILogger<ApproveReferredVacancyReviewCommandHandler> logger, 
                            IVacancyReviewRepository vacancyReviewRepository, 
                            IVacancyRepository vacancyRespository, 
                            IMessaging messaging)
        {
            _logger = logger;
            _vacancyReviewRepository = vacancyReviewRepository;
            _vacancyRepository = vacancyRespository;
            _messaging = messaging;
        }

        public async Task Handle(ApproveReferredVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Approving referred review {reviewId} for vacancy {vacancyReference}.", message.ReviewId, message.Vacancy.VacancyReference);
            
            var vacancy = await _vacancyRepository.GetVacancyAsync(message.Vacancy.Id);

            if (!vacancy.CanApprove)
            {
                _logger.LogWarning($"Unable to approve review {{reviewId}} for vacancy {{vacancyReference}} due to vacancy having a status of {vacancy.Status}.", message.ReviewId, vacancy.VacancyReference);
                return;
            }
            
            var review = await _vacancyReviewRepository.GetAsync(message.ReviewId);

            if (!review.CanApprove)
            {
                _logger.LogWarning($"Unable to approve review {{reviewId}} for vacancy {{vacancyReference}} due to review having a status of {review.Status}.", message.ReviewId, vacancy.VacancyReference);
                return;
            }

            review.ManualOutcome = ManualQaOutcome.Approved;
            review.Status = ReviewStatus.Closed;

            await _vacancyRepository.UpdateAsync(message.Vacancy);

            await _vacancyReviewRepository.UpdateAsync(review);

            await _messaging.PublishEvent(new VacancyReviewApprovedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                ReviewId = message.ReviewId,
                VacancyReference = message.Vacancy.VacancyReference.Value
            });
        }
    }
}
