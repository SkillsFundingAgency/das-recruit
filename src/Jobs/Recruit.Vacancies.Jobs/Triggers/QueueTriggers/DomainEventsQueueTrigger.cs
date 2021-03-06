using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class DomainEventsQueueTrigger
    {
        private readonly ILogger<DomainEventsQueueTrigger> _logger;
        private readonly IDictionary<string, IDomainEventHandler<IEvent>> _handlerLookup;

        public DomainEventsQueueTrigger(ILogger<DomainEventsQueueTrigger> logger, IEnumerable<IDomainEventHandler<IEvent>> handlers)
        {
            _logger = logger;
            _handlerLookup = BuildHandlerLookup(handlers);
        }

        public async Task HandleApplicationSubmittedEvent([QueueTrigger(QueueNames.ApplicationSubmittedQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            await ExecuteHandler(nameof(ApplicationSubmittedEvent), message);
        }

        public async Task HandleApplicationWithdrawnEvent([QueueTrigger(QueueNames.ApplicationWithdrawnQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            await ExecuteHandler(nameof(ApplicationWithdrawnEvent), message);
        }

        public async Task HandleCandidateDeleteEvent([QueueTrigger(QueueNames.CandidateDeleteQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            await ExecuteHandler(nameof(CandidateDeleteEvent), message);
        }

        public async Task HandleVacancyEvent([QueueTrigger(QueueNames.DomainEventsQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            try
            {
                var eventItem = JsonConvert.DeserializeObject<EventItem>(message);

                await ExecuteHandler(eventItem.EventType, eventItem.Data);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to deserialize event: {eventBody}", message);
                throw;
            }
        }

        private async Task ExecuteHandler(string eventType, string data)
        {
            var handler = _handlerLookup[eventType];

            await handler.HandleAsync(data);
        }

        private IDictionary<string, IDomainEventHandler<IEvent>> BuildHandlerLookup(IEnumerable<IDomainEventHandler<IEvent>> handlers)
        {
            var lookup = new Dictionary<string, IDomainEventHandler<IEvent>>();

            foreach(var handler in handlers)
            {
                var eventName = handler.GetType()
                                .GetInterfaces()
                                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                                .GenericTypeArguments[0].Name;

                lookup.Add(eventName, handler);
            }

            return lookup;
        }
    }
}

