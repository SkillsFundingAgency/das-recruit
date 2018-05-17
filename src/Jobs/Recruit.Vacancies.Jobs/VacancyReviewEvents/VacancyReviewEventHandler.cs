using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyReviewEvents
{
    public class VacancyReviewEventHandler
    {
        private readonly IJobsVacancyClient _client;
        private readonly ILogger<VacancyReviewEventHandler> _logger;

        public VacancyReviewEventHandler(IJobsVacancyClient client, ILogger<VacancyReviewEventHandler> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task Handle(VacancyReviewApprovedEvent @event)
        {
            _logger.LogInformation($"Processing {nameof(VacancyReviewApprovedEvent)} for review: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);
            
            await _client.ApproveVacancy(@event.VacancyReference);

            _logger.LogInformation($"Finished Processing {nameof(VacancyCreatedEvent)} for review: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);
        }

        public async Task Handle(VacancyReviewReferredEvent @event)
        {
            _logger.LogInformation($"Processing {nameof(VacancyReviewReferredEvent)} for referral: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);
            
            await _client.ReferVacancy(@event.VacancyReference);

            _logger.LogInformation($"Finished Processing {nameof(VacancyReviewReferredEvent)} for referral: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);
        }
        
    }
}

