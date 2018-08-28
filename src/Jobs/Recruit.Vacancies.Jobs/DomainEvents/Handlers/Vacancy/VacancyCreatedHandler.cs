using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class VacancyCreatedHandler : DomainEventHandler, IDomainEventHandler<VacancyCreatedEvent>
    {
        private readonly ILogger<VacancyCreatedHandler> _logger;
        private readonly IJobsVacancyClient _client;

        public VacancyCreatedHandler(ILogger<VacancyCreatedHandler> logger, IJobsVacancyClient client) : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyCreatedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyCreatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
                await _client.AssignVacancyNumber(@event.VacancyId);
                
                _logger.LogInformation($"Finished Processing {nameof(VacancyCreatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

