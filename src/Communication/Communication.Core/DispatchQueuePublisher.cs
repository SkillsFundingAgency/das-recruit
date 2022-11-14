using System.Threading.Tasks;
using Communication.Types;
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
            var client = new QueueClient(_storageConnectionString, QueueName);
            
            await client.CreateIfNotExistsAsync();

            var cloudMessage = JsonConvert.SerializeObject(message, Formatting.Indented);

            await client.SendMessageAsync(System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cloudMessage)));
        }
    }
}