using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview
{
    public class VacancyReviewApprovedHandler : DomainEventHandler, IDomainEventHandler<VacancyReviewApprovedEvent>
    {
        private readonly ILogger<VacancyReviewApprovedHandler> _logger;
        private readonly IJobsVacancyClient _client;

        public VacancyReviewApprovedHandler(ILogger<VacancyReviewApprovedHandler> logger, IJobsVacancyClient client) : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyReviewApprovedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyReviewApprovedEvent)} for review: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);
                
                await _client.ApproveVacancy(@event.VacancyReference);

                _logger.LogInformation($"Finished Processing {nameof(VacancyCreatedEvent)} for review: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

