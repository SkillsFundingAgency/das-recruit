using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class DraftVacancyUpdatedHandler : DomainEventHandler,  IDomainEventHandler<DraftVacancyUpdatedEvent>
    {
        private readonly ILogger<DraftVacancyUpdatedHandler> _logger;
        private readonly IJobsVacancyClient _client;

        public DraftVacancyUpdatedHandler(ILogger<DraftVacancyUpdatedHandler> logger, IJobsVacancyClient client) : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<DraftVacancyUpdatedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(DraftVacancyUpdatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);

                await _client.AssignVacancyNumber(@event.VacancyId);

                await _client.PatchTrainingProviderAsync(@event.VacancyId);

                _logger.LogInformation($"Finished Processing {nameof(DraftVacancyUpdatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}