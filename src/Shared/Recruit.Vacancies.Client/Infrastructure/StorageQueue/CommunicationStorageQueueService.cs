using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    internal class CommunicationStorageQueueService : StorageQueueServiceBase, ICommunicationQueueService
    {
        private const string QueueName = "communication-requests-queue";
        protected override string ConnectionString { get; }

        public CommunicationStorageQueueService(string connString)
        {
            ConnectionString = connString;
        }

        public override async Task AddMessageAsync<T>(T message)
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var client = storageAccount.CreateCloudQueueClient();

            var queue = client.GetQueueReference(QueueName);

            await AddMessageToQueueAsync(queue, message);
        }
    }
}