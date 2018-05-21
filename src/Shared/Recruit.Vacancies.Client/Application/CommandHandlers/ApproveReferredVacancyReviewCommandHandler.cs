using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApproveReferredVacancyReviewCommandHandler: IRequestHandler<ApproveReferredVacancyReviewCommand>
    {
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMessaging _messaging;

        public ApproveReferredVacancyReviewCommandHandler(IVacancyReviewRepository vacancyReviewRepository, IVacancyRepository vacancyRespository, IMessaging messaging)
        {
            _vacancyReviewRepository = vacancyReviewRepository;
            _vacancyRepository = vacancyRespository;
            _messaging = messaging;
        }

        public async Task Handle(ApproveReferredVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(message.Vacancy.Id);

            if (!vacancy.CanApprove)
            {
                return;
            }

            await _vacancyRepository.UpdateAsync(message.Vacancy);
            
            var review = await _vacancyReviewRepository.GetAsync(message.ReviewId);

            if (review.Status != ReviewStatus.UnderReview)
                throw new InvalidStateException($"Review not in correct state to approve. State: {review.Status}");

            review.ManualOutcome = ManualQaOutcome.Approved;
            review.Status = ReviewStatus.Closed;

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
