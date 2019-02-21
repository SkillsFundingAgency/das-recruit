using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider
{
    public class SetupProviderHandler : DomainEventHandler,  IDomainEventHandler<SetupProviderEvent>
    {
        private readonly ILogger<SetupProviderHandler> _logger;
        private readonly IEditVacancyInfoProjectionService _projectionService;
        private readonly IProviderRelationshipsService _providerRelationshipService;

        public SetupProviderHandler(ILogger<SetupProviderHandler> logger, 
            IEditVacancyInfoProjectionService projectionService,
            IProviderRelationshipsService providerRelationshipService) : base(logger)
        {
            _logger = logger;
            _projectionService = projectionService;
            _providerRelationshipService = providerRelationshipService;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var eventData = DeserializeEvent<SetupProviderEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(SetupProviderEvent)} for Ukprn: {{Ukprn}}", eventData.Ukprn);

                var employerInfos = await _providerRelationshipService.GetLegalEntitiesForProviderAsync(eventData.Ukprn);

                await _projectionService.UpdateProviderVacancyDataAsync(eventData.Ukprn, employerInfos);

                _logger.LogInformation($"Finished Processing {nameof(SetupProviderEvent)} for Ukprn: {{Ukprn}}", eventData.Ukprn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", eventData);
                throw;
            }
        }

        
    }
}