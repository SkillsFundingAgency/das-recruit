using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Events;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApproveVacancyReviewCommandHandler: IRequestHandler<ApproveVacancyReviewCommand>
    {
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMessaging _messaging;

        public ApproveVacancyReviewCommandHandler(IVacancyReviewRepository vacancyReviewRepository, IVacancyRepository vacancyRespository, IMessaging messaging)
        {
            _vacancyReviewRepository = vacancyReviewRepository;
            _vacancyRepository = vacancyRespository;
            _messaging = messaging;
        }

        public async Task Handle(ApproveVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            var review = await _vacancyReviewRepository.GetAsync(message.VacancyReference);
            review.ManualOutcome = ManualQaOutcome.Approved;
            review.Status = ReviewStatus.Closed;

            await _vacancyReviewRepository.UpdateAsync(review);

            // TODO: LWA are we happy about getting vacancy details for this 
            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyReference);

            await _messaging.PublishEvent(new VacancyApprovedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id,
                VacancyReference = message.VacancyReference
            });
        }
    }
}
