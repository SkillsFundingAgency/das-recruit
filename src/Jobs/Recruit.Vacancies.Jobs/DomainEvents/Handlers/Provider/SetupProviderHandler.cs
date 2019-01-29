using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider
{
    public class SetupProviderHandler : DomainEventHandler,  IDomainEventHandler<SetupProviderEvent>
    {
        private readonly ILogger<SetupProviderHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly IEditVacancyInfoProjectionService _projectionService;

        public SetupProviderHandler(ILogger<SetupProviderHandler> logger, 
            IJobsVacancyClient client, 
            IEditVacancyInfoProjectionService projectionService) : base(logger)
        {
            _logger = logger;
            _client = client;
            _projectionService = projectionService;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<SetupProviderEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(SetupProviderEvent)} for Ukprn: {{Ukprn}}", @event.Ukprn);

                // we need to work out what we want to store for provider e.g. employer legal entities they have relationships with
                // var legalEntities = (await _client.GetEmployerLegalEntitiesAsync(@event.EmployerAccountId)).ToList();

                // var vacancyDataTask =  _projectionService.UpdateEmployerVacancyDataAsync(@event.EmployerAccountId, legalEntities);

                // var employerProfilesTask = _client.RefreshEmployerProfiles(@event.EmployerAccountId, legalEntities.Select(x => x.LegalEntityId));

                // await Task.WhenAll(vacancyDataTask, employerProfilesTask);
                await Task.CompletedTask;

                _logger.LogInformation($"Finished Processing {nameof(SetupProviderEvent)} for Ukprn: {{Ukprn}}", @event.Ukprn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}