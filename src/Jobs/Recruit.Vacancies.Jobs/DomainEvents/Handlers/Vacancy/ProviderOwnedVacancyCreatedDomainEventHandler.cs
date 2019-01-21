using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class ProviderOwnedVacancyCreatedDomainEventHandler : DomainEventHandler, IDomainEventHandler<VacancyClonedEvent>
    {
        private readonly ILogger<ProviderOwnedVacancyCreatedDomainEventHandler> _logger;
        private readonly IJobsVacancyClient _client;

        public ProviderOwnedVacancyCreatedDomainEventHandler(ILogger<ProviderOwnedVacancyCreatedDomainEventHandler> logger, IJobsVacancyClient client) : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<ProviderOwnedVacancyCreatedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(ProviderOwnedVacancyCreatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);

                await _client.PatchTrainingProvider(@event.VacancyId);
                
                _logger.LogInformation($"Finished Processing {nameof(ProviderOwnedVacancyCreatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

