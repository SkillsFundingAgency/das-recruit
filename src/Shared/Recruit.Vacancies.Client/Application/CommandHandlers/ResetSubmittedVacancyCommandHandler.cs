using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ResetSubmittedVacancyCommandHandler : IRequestHandler<ResetSubmittedVacancyCommand, Unit>
    {
        private readonly ILogger<ResetSubmittedVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IVacancyReviewQuery _vacancyReviewQuery;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;
        public ResetSubmittedVacancyCommandHandler(
            IVacancyRepository vacancyRepository,
            IVacancyReviewQuery vacancyReviewQuery,
            IVacancyReviewRepository vacancyReviewRepository,
            ITimeProvider timeProvider,
            IMessaging messaging,
            ILogger<ResetSubmittedVacancyCommandHandler> logger)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _vacancyReviewQuery = vacancyReviewQuery;
            _vacancyReviewRepository = vacancyReviewRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }
        public async Task<Unit> Handle(ResetSubmittedVacancyCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyId);
            if (vacancy.Status != VacancyStatus.Submitted)
            {
                _logger.LogInformation($"No reviews will be updated for the vacancy {vacancy.VacancyReference} as it is in status {vacancy.Status}");
                return Unit.Value;
            }

            var review = await _vacancyReviewQuery.GetLatestReviewByReferenceAsync(vacancy.VacancyReference.GetValueOrDefault());

            if (review == null || review.Status == ReviewStatus.Closed)
            {
                _logger.LogInformation($"No active reviews found for vacancy {vacancy.VacancyReference}");
                return Unit.Value;
            }

            if (review.Status == ReviewStatus.UnderReview)
            {
                _logger.LogInformation($"The vacancy will not be updated {vacancy.VacancyReference} as it is being reviewed {review.Id}.");
                return Unit.Value;
            }
            else if (review.IsPending)
            {
                await ClosePendingReview(review);
                await UpdateVacancyStatusToDraft(vacancy);
                await _messaging.PublishEvent(
                    new VacancyReviewWithdrawnEvent(vacancy.Id, vacancy.VacancyReference.GetValueOrDefault(), review.Id));
            }
            
            return Unit.Value;
        }

        private Task UpdateVacancyStatusToDraft(Vacancy vacancy)
        {
            _logger.LogInformation($"Resetting vacancy {vacancy.VacancyReference} to Draft status.");
            vacancy.Status = VacancyStatus.Draft;
            vacancy.SubmittedDate = null;
            vacancy.SubmittedByUser = null;
            return _vacancyRepository.UpdateAsync(vacancy);
        }

        private Task ClosePendingReview(VacancyReview review)
        {
            _logger.LogInformation($"Closing pending review {review.Id} as the provider is blocked");
            review.ManualOutcome = ManualQaOutcome.Blocked;
            review.Status = ReviewStatus.Closed;
            review.ClosedDate = _timeProvider.Now;
            return _vacancyReviewRepository.UpdateAsync(review);
        }
    }
}