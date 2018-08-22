using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Events
{
    internal sealed class StorageQueueEventStore : IEventStore
    {
        private readonly string _connectionString;

        public StorageQueueEventStore(StorageQueueConnectionDetails details)
        {
            _connectionString = details.ConnectionString;
        }

        public async Task Add(IEvent @event)
        {
            var json = JsonConvert.SerializeObject(@event, Formatting.Indented);

            var item = new EventItem
            {
                EventType = @event.GetType().Name,
                Data = json,
                SourceCommandId = @event.SourceCommandId
            };

            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var client = storageAccount.CreateCloudQueueClient();
            
            var queueName = GetQueueName(@event);
            
            var queue = client.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();

            var message = new CloudQueueMessage(JsonConvert.SerializeObject(item, Formatting.Indented));

            await queue.AddMessageAsync(message);
        }

        private static string GetQueueName(IEvent @event)
        {
            switch (@event)
            {
                case IVacancyEvent _:
                    return QueueNames.VacancyEventsQueueName;
                case IVacancyReviewEvent _:
                    return QueueNames.VacancyReviewEventsQueueName;
            }

            return @event.GetType().Name.Replace("Event", "Queue").PascalToKebabCase();
        }
    }
}
