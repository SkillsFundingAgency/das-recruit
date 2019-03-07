using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore
{
    internal sealed class StorageQueueEventStore : StorageQueue.StorageQueue, IEventStore
    {
        public StorageQueueEventStore(StorageQueueConnectionDetails details) : base(details, QueueNames.DomainEventsQueueName)
        {
        }

        public Task Add(IEvent @event)
        {
            var json = JsonConvert.SerializeObject(@event, Formatting.Indented);

            var item = new EventItem
            {
                EventType = @event.GetType().Name,
                Data = json
            };

            return AddMessageAsync(item);
        }
    }
}
