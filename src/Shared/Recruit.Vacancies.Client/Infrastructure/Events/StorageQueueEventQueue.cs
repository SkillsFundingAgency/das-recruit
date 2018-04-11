﻿using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Events
{
    internal sealed class StorageQueueEventQueue : IEventStore
    {
        private readonly string _connectionString;

        public StorageQueueEventQueue(StorageQueueConnectionDetails details)
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
            
            string queueName = GetQueueName(@event);
            
            var queue = client.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();

            var message = new CloudQueueMessage(JsonConvert.SerializeObject(item, Formatting.Indented));

            await queue.AddMessageAsync(message);
        }

        private static string GetQueueName(IEvent @event)
        {
            if (@event is IVacancyEvent)
                return QueueNames.VacancyEventsQueueName;

            return @event.GetType().Name.Replace("Event", "Queue").PascalToKebabCase();
        }
    }
}
