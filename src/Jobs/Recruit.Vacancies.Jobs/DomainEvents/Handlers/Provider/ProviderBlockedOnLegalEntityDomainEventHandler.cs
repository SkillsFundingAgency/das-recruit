using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider
{
    public class ProviderBlockedOnLegalEntityDomainEventHandler : DomainEventHandler, IDomainEventHandler<ProviderBlockedOnLegalEntityEvent>
    {
        ILogger<ProviderBlockedOnLegalEntityDomainEventHandler> _logger;
        public ProviderBlockedOnLegalEntityDomainEventHandler(ILogger<ProviderBlockedOnLegalEntityDomainEventHandler> logger) : base(logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(string eventPayload)
        {
            var eventData = DeserializeEvent<ProviderBlockedOnLegalEntityEvent>(eventPayload);
            //TODO 
            // call provider permissions api to revoke permission for each legal entity
            return Task.CompletedTask;
        }
    }
}