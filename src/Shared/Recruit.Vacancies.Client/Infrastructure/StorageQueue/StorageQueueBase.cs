using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    internal abstract class StorageQueueBase
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        protected StorageQueueBase(StorageQueueConnectionDetails details, string queueName)
        {
            _connectionString = details.ConnectionString;
            _queueName = queueName;
        }

        protected async Task AddMessageAsync(object content)
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var client = storageAccount.CreateCloudQueueClient();
            
            var queue = client.GetQueueReference(_queueName);
            await queue.CreateIfNotExistsAsync();

            var message = new CloudQueueMessage(JsonConvert.SerializeObject(content, Formatting.Indented));

            await queue.AddMessageAsync(message);
        }
    }
}
