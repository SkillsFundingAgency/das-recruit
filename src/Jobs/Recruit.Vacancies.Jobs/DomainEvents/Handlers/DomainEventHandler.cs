using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers
{
    public abstract class DomainEventHandler
    {
        private readonly ILogger _logger;

        protected DomainEventHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected T DeserializeEvent<T>(string eventPayload)
        {
            try
            {
                var evt = JsonConvert.DeserializeObject<T>(eventPayload);
                return evt;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to deserialize event: {eventBody}", eventPayload);
                throw;
            }
        }
    }
}

