using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview
{
    public class VacancyReviewReferredHandler(ILogger<VacancyReviewReferredHandler> logger, IJobsVacancyClient client)
        : DomainEventHandler(logger), IDomainEventHandler<VacancyReviewReferredEvent>
    {
        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyReviewReferredEvent>(eventPayload);

            try
            {
                logger.LogInformation("Processing {EventName} for referral: {ReviewId} vacancy: {VacancyReference}", nameof(VacancyReviewReferredEvent), @event.ReviewId, @event.VacancyReference);

                await client.ReferVacancyAsync(@event.VacancyReference);

                logger.LogInformation("Finished Processing {EventName} for referral: {ReviewId} vacancy: {VacancyReference}", nameof(VacancyReviewReferredEvent), @event.ReviewId, @event.VacancyReference);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to process {EventBody}", @event);
                throw;
            }
        }
    }
}