using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CreateVacancyReviewCommandHandler: IRequestHandler<CreateVacancyReviewCommand>
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _time;

        public CreateVacancyReviewCommandHandler(IVacancyRepository vacancyRepository, IVacancyReviewRepository vacancyReviewRepository, IMessaging messaging, ITimeProvider time)
        {
            _vacancyRepository = vacancyRepository;
            _vacancyReviewRepository = vacancyReviewRepository;
            _messaging = messaging;
            _time = time;
        }

        public async Task Handle(CreateVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyReference);

            var review = BuildNewReview(vacancy);

            await _vacancyReviewRepository.CreateAsync(review);

            await _messaging.PublishEvent(new VacancyReviewCreatedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                VacancyReference = message.VacancyReference,
                ReviewId =  review.Id
            });
        }

        private VacancyReview BuildNewReview(Vacancy vacancy)
        {
            var review = new VacancyReview
            {
                VacancyReference = vacancy.VacancyReference.Value,
                Title = vacancy.Title,
                Status = ReviewStatus.PendingReview,    // NOTE: This is temporary for private beta.
                CreatedDate = _time.Now,                // NOTE: This is temporary for private beta.
                EmployerAccountId = vacancy.EmployerAccountId,
                SubmittedByUser = vacancy.SubmittedByUser
            };

            return review;
        }
    }
}
