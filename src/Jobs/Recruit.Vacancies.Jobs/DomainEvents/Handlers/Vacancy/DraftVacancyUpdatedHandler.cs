using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class DraftVacancyUpdatedHandler(ILogger<DraftVacancyUpdatedHandler> logger, IJobsVacancyClient client)
        : DomainEventHandler(logger), IDomainEventHandler<DraftVacancyUpdatedEvent>
    {
        private const string EventName = nameof(DraftVacancyUpdatedEvent);
        
        public async Task HandleAsync(string eventPayload)
        {
            var draftVacancyUpdatedEvent = DeserializeEvent<DraftVacancyUpdatedEvent>(eventPayload);
            try
            {
                logger.LogInformation("Processing {EventName} for vacancy: {VacancyId}", EventName, draftVacancyUpdatedEvent.VacancyId);

                await client.AssignVacancyNumber(draftVacancyUpdatedEvent.VacancyId);
                await client.PatchTrainingProviderAsync(draftVacancyUpdatedEvent.VacancyId);

                logger.LogInformation("Finished Processing {EventName} for vacancy: {VacancyId}", EventName, draftVacancyUpdatedEvent.VacancyId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to process {eventBody}", draftVacancyUpdatedEvent);
                throw;
            }
        }
    }
}