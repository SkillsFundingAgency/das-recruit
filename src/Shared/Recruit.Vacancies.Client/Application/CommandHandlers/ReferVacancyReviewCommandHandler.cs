using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ReferVacancyReviewCommandHandler : IRequestHandler<ReferVacancyReviewCommand>
    {
        private readonly IVacancyReviewRepository _reviewRepository;
        private readonly IMessaging _messaging;

        public ReferVacancyReviewCommandHandler(IVacancyReviewRepository reviewRepository, IMessaging messaging)
        {
            _reviewRepository = reviewRepository;
            _messaging = messaging;
        }

        public async Task Handle(ReferVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetAsync(message.ReviewId);

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
