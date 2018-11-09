using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class VacancyClonedDomainEventHandler : DomainEventHandler, IDomainEventHandler<VacancyClonedEvent>
    {
        private readonly ILogger<VacancyClonedDomainEventHandler> _logger;
        private readonly IJobsVacancyClient _client;

        public VacancyClonedDomainEventHandler(ILogger<VacancyClonedDomainEventHandler> logger, IJobsVacancyClient client) : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyCreatedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyClonedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
                
                await _client.AssignVacancyNumber(@event.VacancyId);
                
                _logger.LogInformation($"Finished Processing {nameof(VacancyClonedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

