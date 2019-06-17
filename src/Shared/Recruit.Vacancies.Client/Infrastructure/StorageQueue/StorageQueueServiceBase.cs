using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    public abstract class StorageQueueServiceBase : IQueueService
    {
        protected abstract string ConnectionString { get; }

        protected async Task AddMessageToQueueAsync<T>(CloudQueue queue, T message)
        {
            await queue.CreateIfNotExistsAsync();

            var cloudMessage = new CloudQueueMessage(JsonConvert.SerializeObject(message, Formatting.Indented));

            await queue.AddMessageAsync(cloudMessage);
        }

        public abstract Task AddMessageAsync<T>(T message);
    }
}