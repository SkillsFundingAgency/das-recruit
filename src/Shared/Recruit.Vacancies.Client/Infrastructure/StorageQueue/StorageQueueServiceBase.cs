using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    public abstract class StorageQueueServiceBase
    {
        protected abstract string ConnectionString { get; }

        protected async Task AddMessageToQueueAsync<T>(QueueClient queueClient, T message)
        {
            await queueClient.CreateIfNotExistsAsync();

            string jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await queueClient.SendMessageAsync(jsonMessage);
        }

        public abstract Task AddMessageAsync<T>(T message);
    }
}