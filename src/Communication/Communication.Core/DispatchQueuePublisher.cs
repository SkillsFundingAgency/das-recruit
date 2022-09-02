using System.Threading.Tasks;
using Communication.Types;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Newtonsoft.Json;

namespace Communication.Core
{
    public class DispatchQueuePublisher : IDispatchQueuePublisher
    {
        private const string QueueName = "communication-messages-dispatcher-queue";
        private readonly string _storageConnectionString;

        public DispatchQueuePublisher(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
        }

        public async Task AddMessageAsync(CommunicationMessageIdentifier message)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();

            var queue = client.GetQueueReference(QueueName);

            await queue.CreateIfNotExistsAsync();

            var cloudMessage = new CloudQueueMessage(JsonConvert.SerializeObject(message, Formatting.Indented));

            await queue.AddMessageAsync(cloudMessage);
        }
    }
}