using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider
{
    public class ProviderBlockedOnLegalEntityDomainEventHandler : DomainEventHandler, IDomainEventHandler<ProviderBlockedOnLegalEntityEvent>
    {
        private readonly IProviderRelationshipsService _providerRelationshipsService;
        private readonly ILogger<ProviderBlockedOnLegalEntityDomainEventHandler> _logger;
        public ProviderBlockedOnLegalEntityDomainEventHandler(IProviderRelationshipsService providerRelationshipsService,
            ILogger<ProviderBlockedOnLegalEntityDomainEventHandler> logger) : base(logger)
        {
            _logger = logger;
            _providerRelationshipsService = providerRelationshipsService;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var eventData = DeserializeEvent<ProviderBlockedOnLegalEntityEvent>(eventPayload);
            
            _logger.LogInformation($"Attempting to revoke provider {eventData.Ukprn} permission on account {eventData.EmployerAccountId} with public hash {eventData.AccountLegalEntityPublicHashedId} for legal entity {eventData.AccountLegalEntityPublicHashedId}.");

            await _providerRelationshipsService.RevokeProviderPermissionToRecruitAsync(eventData.Ukprn, eventData.AccountLegalEntityPublicHashedId);

            _logger.LogInformation($"Successfully revoked provider {eventData.Ukprn} permission on account {eventData.EmployerAccountId} for legal entity {eventData.AccountLegalEntityPublicHashedId}.");
        }
    }
}