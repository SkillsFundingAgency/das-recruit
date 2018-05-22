﻿using Esfa.Recruit.Vacancies.Client.Application.Commands;
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
    public class ApproveVacancyReviewCommandHandler: IRequestHandler<ApproveVacancyReviewCommand>
    {
        private readonly ILogger<ApproveVacancyReviewCommandHandler> _logger;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IMessaging _messaging;

        public ApproveVacancyReviewCommandHandler(ILogger<ApproveVacancyReviewCommandHandler> logger, 
                                        IVacancyReviewRepository vacancyReviewRepository, 
                                        IMessaging messaging)
        {
            _logger = logger;
            _vacancyReviewRepository = vacancyReviewRepository;
            _messaging = messaging;
        }

        public async Task Handle(ApproveVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Approving review {reviewId}.", message.ReviewId);

            var review = await _vacancyReviewRepository.GetAsync(message.ReviewId);

            if (!review.CanApprove)
            {
                _logger.LogWarning($"Unable to approve review {{reviewId}} due to review having a status of {review.Status}.", message.ReviewId);
                return;
            }
            review.ManualOutcome = ManualQaOutcome.Approved;
            review.Status = ReviewStatus.Closed;

            await _vacancyReviewRepository.UpdateAsync(review);

            await _messaging.PublishEvent(new VacancyReviewApprovedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                ReviewId = message.ReviewId,
                VacancyReference = review.VacancyReference
            });
        }
    }
}
