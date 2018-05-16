using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyEvents
{
    public class VacancyEventHandler
    {
        private readonly IJobsVacancyClient _client;
        private readonly ILogger<VacancyEventHandler> _logger;

        public VacancyEventHandler(IJobsVacancyClient client, ILogger<VacancyEventHandler> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task Handle(VacancyCreatedEvent @event)
        {
            _logger.LogInformation($"Processing {nameof(VacancyCreatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            
            await _client.AssignVacancyNumber(@event.VacancyId);

            _logger.LogInformation($"Finished Processing {nameof(VacancyCreatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
        }

        public Task Handle(VacancyUpdatedEvent @event)
        {
            throw new NotImplementedException();
        }

        public async Task Handle(VacancySubmittedEvent @event)
        {
            _logger.LogInformation($"Processing {nameof(VacancySubmittedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            
            await _client.CreateVacancyReview(@event.VacancyReference);

            _logger.LogInformation($"Finished Processing {nameof(VacancySubmittedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
        }

        public Task Handle(VacancyDeletedEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}

