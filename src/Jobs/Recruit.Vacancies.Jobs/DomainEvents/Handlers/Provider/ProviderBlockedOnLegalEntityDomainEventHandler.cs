using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider
{
    public class ProviderBlockedOnLegalEntityDomainEventHandler : DomainEventHandler, IDomainEventHandler<ProviderBlockedOnLegalEntityEvent>
    {
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private readonly IProviderRelationshipsService _providerRelationshipsService;
        private readonly ILogger<ProviderBlockedOnLegalEntityDomainEventHandler> _logger;
        public ProviderBlockedOnLegalEntityDomainEventHandler(
            IEmployerAccountProvider employerAccountProvider,
            IProviderRelationshipsService providerRelationshipsService,
            ILogger<ProviderBlockedOnLegalEntityDomainEventHandler> logger) : base(logger)
        {
            _logger = logger;
            _employerAccountProvider = employerAccountProvider;
            _providerRelationshipsService = providerRelationshipsService;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var eventData = DeserializeEvent<ProviderBlockedOnLegalEntityEvent>(eventPayload);
            
            _logger.LogInformation($"Attempting to revoke provider {eventData.Ukprn} permission on account {eventData.EmployerAccountId} with public hash {eventData.AccountLegalEntityPublicHashedId} for legal entity {eventData.LegalEntityId}.");

            await _providerRelationshipsService.RevokeProviderPermissionToRecruitAsync(eventData.Ukprn, eventData.AccountLegalEntityPublicHashedId);

            _logger.LogInformation($"Successfully revoked provider {eventData.Ukprn} permission on account {eventData.EmployerAccountId} for legal entity {eventData.LegalEntityId}.");
        }

        private async Task<string> GetAccountLegalEntityPublicHashId(ProviderBlockedOnLegalEntityEvent eventData)
        {
            var legalEntities = await _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(eventData.EmployerAccountId);

            return legalEntities.FirstOrDefault(l => l.LegalEntityId == eventData.LegalEntityId).AccountLegalEntityPublicHashedId;
        }
    }
}