using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Entities = Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class ProviderBlockedOnVacancyDomainEventHandler : DomainEventHandler, IDomainEventHandler<ProviderBlockedOnVacancyEvent>
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IVacancyReviewQuery _vacancyReviewQuery;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IVacancyService _vacancyService;
        private readonly ILogger<ProviderBlockedOnVacancyDomainEventHandler> _logger;
        public ProviderBlockedOnVacancyDomainEventHandler(
            IVacancyRepository vacancyRepository,
            IVacancyReviewQuery vacancyReviewQuery,
            IVacancyReviewRepository vacancyReviewRepository,
            ITimeProvider timeProvider,
            IVacancyService vacancyService,
            ILogger<ProviderBlockedOnVacancyDomainEventHandler> logger) : base(logger)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _vacancyReviewQuery = vacancyReviewQuery;
            _vacancyReviewRepository = vacancyReviewRepository;
            _timeProvider = timeProvider;
            _vacancyService = vacancyService;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var eventData = DeserializeEvent<ProviderBlockedOnVacancyEvent>(eventPayload);

            _logger.LogInformation($"Updating vacancy {eventData.VacancyId} as the provider {eventData.Ukprn} is blocked");

            var vacancy = await _vacancyRepository.GetVacancyAsync(eventData.VacancyId);

            var isVacancyUpdated = false;

            if (vacancy.OwnerType == Entities.OwnerType.Provider && vacancy.TransferInfo == null)
            {
                _logger.LogInformation($"Transferring the vacancy {vacancy.VacancyReference} to the employer as the provider {eventData.Ukprn} is blocked");
                isVacancyUpdated = true;
                vacancy.TransferInfo = new Entities.TransferInfo()
                {
                    Ukprn = eventData.Ukprn,
                    ProviderName = vacancy.TrainingProvider.Name,
                    LegalEntityName = vacancy.LegalEntityName,
                    TransferredByUser = eventData.QaVacancyUser,
                    TransferredDate = eventData.BlockedDate,
                    Reason = Entities.TransferReason.BlockedByQa
                };
                vacancy.OwnerType = Entities.OwnerType.Employer;
            }

            if(vacancy.Status == Entities.VacancyStatus.Submitted)
            {
                var review = await _vacancyReviewQuery.GetLatestReviewByReferenceAsync(vacancy.VacancyReference.GetValueOrDefault());
                if (review.IsPending)
                {
                    await ClosePendingReview(review);
                    isVacancyUpdated = true;
                    vacancy.Status = Entities.VacancyStatus.Draft;
                    vacancy.LastUpdatedByUser = eventData.QaVacancyUser;
                    vacancy.LastUpdatedDate = eventData.BlockedDate;
                }
                else
                {
                    _logger.LogInformation($"The vacancy {vacancy.VacancyReference} is under review and cannot be closed.");
                }
            }

            if(isVacancyUpdated)
            {
                await _vacancyRepository.UpdateAsync(vacancy);
            }

            if(vacancy.Status == Entities.VacancyStatus.Live)
            {
                _logger.LogInformation($"Closing live vacancy {eventData.VacancyId} as the provider {eventData.Ukprn} is blocked");
                await _vacancyService.CloseVacancyImmediately(eventData.VacancyId, eventData.QaVacancyUser, Entities.ClosureReason.BlockedByQa);
            }
        }

        private async Task ClosePendingReview(Entities.VacancyReview review)
        {
            _logger.LogInformation($"Closing pending review {review.Id} as the provider is blocked");
            review.ManualOutcome = Entities.ManualQaOutcome.Withdrawn;
            review.Status = Entities.ReviewStatus.Closed;
            review.ClosedDate = _timeProvider.Now;

            await _vacancyReviewRepository.UpdateAsync(review);
        }
    }
}