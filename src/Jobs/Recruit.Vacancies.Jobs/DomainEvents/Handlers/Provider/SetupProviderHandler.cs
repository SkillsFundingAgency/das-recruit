using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider
{
    public class SetupProviderHandler(
        ILogger<SetupProviderHandler> logger,
        IProviderRelationshipsService providerRelationshipService,
        IEmployerVacancyClient client,
        IGetProviderStatusClient providerStatusClient)
        : DomainEventHandler(logger), IDomainEventHandler<SetupProviderEvent>
    {
        public async Task HandleAsync(string eventPayload)
        {
            var eventData = DeserializeEvent<SetupProviderEvent>(eventPayload);

            try
            {
                logger.LogInformation($"Processing {nameof(SetupProviderEvent)} for Ukprn: {{Ukprn}}", eventData.Ukprn);

                var employerInfosTask = providerRelationshipService.GetLegalEntitiesForProvider(eventData.Ukprn,"", [OperationType.Recruitment]);
                var providerStatus = providerStatusClient.GetProviderStatus(eventData.Ukprn);

                await Task.WhenAll(employerInfosTask, providerStatus);

                var employerInfos = employerInfosTask.Result;
                
                foreach (var employerInfo in employerInfos)
                {
                    await client.SetupEmployerAsync(employerInfo.EmployerAccountId);
                }

                logger.LogInformation("Finished Processing {SetupProviderEventName} for Ukprn: {Ukprn} has agreement:{ProviderAccountResponse}", nameof(SetupProviderEvent), eventData.Ukprn, providerStatus.Result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to process {EventBody}", eventData);
                throw;
            }
        }
    }
}