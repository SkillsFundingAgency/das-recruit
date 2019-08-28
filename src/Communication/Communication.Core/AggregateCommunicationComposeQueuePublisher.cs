using System.Threading.Tasks;
using Communication.Types;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Communication.Core
{
    public class AggregateCommunicationComposeQueuePublisher : IAggregateCommunicationComposeQueuePublisher
    {
        private const string QueueName = "communication-messages-composer-queue";
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