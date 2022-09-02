using System.Threading.Tasks;
using Communication.Types;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;

namespace Communication.Core
{
    public class AggregateCommunicationComposeQueuePublisher : IAggregateCommunicationComposeQueuePublisher
    {
        private const string QueueName = "aggregate-communication-composer-requests-queue";
        private readonly string _storageConnectionString;

        public AggregateCommunicationComposeQueuePublisher(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
        }

        public async Task AddMessageAsync(AggregateCommunicationComposeRequest message)
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