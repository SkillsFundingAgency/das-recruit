using System.Threading.Tasks;
using Communication.Types;
using Newtonsoft.Json;
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
            var client = new QueueClient(_storageConnectionString, QueueName);
            
            await client.CreateIfNotExistsAsync();

            var cloudMessage = JsonConvert.SerializeObject(message, Formatting.Indented);

            await client.SendMessageAsync(System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cloudMessage)));
        }
    }
}