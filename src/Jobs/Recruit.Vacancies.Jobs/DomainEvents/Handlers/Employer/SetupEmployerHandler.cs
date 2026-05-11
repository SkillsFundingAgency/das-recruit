using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Employer
{
    public class SetupEmployerHandler(
        ILogger<SetupEmployerHandler> logger,
        IJobsVacancyClient client,
        IEditVacancyInfoProjectionService projectionService)
        : DomainEventHandler(logger), IDomainEventHandler<SetupEmployerEvent>
    {
        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<SetupEmployerEvent>(eventPayload);

            try
            {
                logger.LogInformation($"Processing {nameof(SetupEmployerEvent)} for Account: {{AccountId}}", @event.EmployerAccountId);

                var legalEntities = (await client.GetEmployerLegalEntitiesAsync(@event.EmployerAccountId)).ToList();

                var vacancyDataTask = projectionService.UpdateEmployerVacancyDataAsync(@event.EmployerAccountId, legalEntities);

                var employerProfilesTask = client.RefreshEmployerProfiles(@event.EmployerAccountId, legalEntities.Select(x => x.AccountLegalEntityPublicHashedId));

                await Task.WhenAll(vacancyDataTask, employerProfilesTask);

                logger.LogInformation($"Finished Processing {nameof(SetupEmployerEvent)} for Account: {{AccountId}}", @event.EmployerAccountId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}