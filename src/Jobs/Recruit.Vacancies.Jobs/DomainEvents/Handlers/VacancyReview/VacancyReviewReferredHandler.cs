using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview
{
    public class VacancyReviewReferredHandler : DomainEventHandler, IDomainEventHandler<VacancyReviewReferredEvent>
    {
        private readonly IJobsVacancyClient _client;
        private readonly ILogger<VacancyReviewReferredHandler> _logger;

        public VacancyReviewReferredHandler(ILogger<VacancyReviewReferredHandler> logger, IJobsVacancyClient client) : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyReviewReferredEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyReviewReferredEvent)} for referral: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);

                await _client.ReferVacancyAsync(@event.VacancyReference);

                _logger.LogInformation($"Finished Processing {nameof(VacancyReviewReferredEvent)} for referral: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}