using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Azure.Storage.Queues;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    internal class CommunicationStorageQueueService : StorageQueueServiceBase, ICommunicationQueueService
    {
        private readonly IDictionary<Type, string> _messageToCommunicationStorageQueueMapper = new Dictionary<Type, string>
        {
            { typeof(CommunicationRequest), "communication-requests-queue" },
            { typeof(AggregateCommunicationRequest), "aggregate-communication-requests-queue" }
        };

        protected override string ConnectionString { get; }

        public CommunicationStorageQueueService(string connString)
        {
            ConnectionString = connString;
        }

        public override async Task AddMessageAsync<T>(T message)
        {
            var queueName = _messageToCommunicationStorageQueueMapper[typeof(T)];

            if(string.IsNullOrEmpty(queueName))
                throw new ArgumentException($"Cannot map type {typeof(T).Name} to a queue name");

            var queueClient = new QueueClient(ConnectionString, queueName);

            await queueClient.CreateIfNotExistsAsync();

            string messageText = JsonConvert.SerializeObject(message);
            await queueClient.SendMessageAsync(messageText);
        }
    }
}