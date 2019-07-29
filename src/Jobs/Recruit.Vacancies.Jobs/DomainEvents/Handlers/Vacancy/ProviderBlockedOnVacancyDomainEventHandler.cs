using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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

            var vacancy = await _vacancyRepository.GetVacancyAsync(eventData.VacancyId);

            var isVacancyUpdated = false;

            if (vacancy.OwnerType == OwnerType.Provider && vacancy.TransferInfo == null)
            {
                isVacancyUpdated = true;
                vacancy.TransferInfo = new TransferInfo()
                {
                    Ukprn = eventData.Ukprn,
                    ProviderName = vacancy.TrainingProvider.Name,
                    LegalEntityName = vacancy.LegalEntityName,
                    TransferredByUser = eventData.QaVacancyUser,
                    TransferredDate = eventData.BlockedDate,
                    Reason = TransferReason.BlockedByQa
                };
                vacancy.OwnerType = OwnerType.Employer;
            }

            if(vacancy.Status == VacancyStatus.Submitted)
            {
                await ClosePendingReview(vacancy.VacancyReference.GetValueOrDefault());
                isVacancyUpdated = true;
                vacancy.Status = VacancyStatus.Draft;
                vacancy.LastUpdatedByUser = eventData.QaVacancyUser;
                vacancy.LastUpdatedDate = eventData.BlockedDate;
            }

            if(isVacancyUpdated)
            {
                await _vacancyRepository.UpdateAsync(vacancy);
            }

            if(vacancy.Status == VacancyStatus.Live)
            {
                await _vacancyService.CloseVacancyImmediately(eventData.VacancyId, eventData.QaVacancyUser, ClosureReason.BlockedByQa);
            }
        }

        private async Task ClosePendingReview(long vacancyReference)
        {
            var review = await _vacancyReviewQuery.GetLatestReviewByReferenceAsync(vacancyReference);

            if (review.Status == ReviewStatus.New || review.Status == ReviewStatus.PendingReview)
            {
                review.ManualOutcome = ManualQaOutcome.Withdrawn;
                review.Status = ReviewStatus.Closed;
                review.ClosedDate = _timeProvider.Now;

                await _vacancyReviewRepository.UpdateAsync(review);
            }

        }
    }
}