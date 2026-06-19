using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Employer
{
    public class SetupEmployerHandler(
        ILogger<SetupEmployerHandler> logger,
        IJobsVacancyClient client)
        : DomainEventHandler(logger), IDomainEventHandler<SetupEmployerEvent>
    {
        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<SetupEmployerEvent>(eventPayload);

            try
            {
                logger.LogInformation($"Processing {nameof(SetupEmployerEvent)} for Account: {{AccountId}}", @event.EmployerAccountId);

                var legalEntities = (await client.GetEmployerLegalEntitiesAsync(@event.EmployerAccountId)).ToList();
                await client.RefreshEmployerProfiles(@event.EmployerAccountId, legalEntities.Select(x => x.AccountLegalEntityPublicHashedId));

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