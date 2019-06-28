using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    internal class StorageQueueService : IQueueService
    {
        private readonly Dictionary<Type, string> _messageToStorageQueueMapper = new Dictionary<Type, string> 
        {
            { typeof(DeleteReportsQueueMessage), QueueNames.DeleteReportsQueueName },
            { typeof(EventItem), QueueNames.DomainEventsQueueName },
            { typeof(ReportQueueMessage), QueueNames.ReportQueueName },
            { typeof(UpdateApprenticeshipProgrammesQueueMessage), QueueNames.UpdateApprenticeProgrammesQueueName },
            { typeof(UpdateBankHolidaysQueueMessage), QueueNames.UpdateBankHolidaysQueueName },
            { typeof(UpdateBlockedEmployersQueueMessage), QueueNames.GenerateBlockedEmployersQueueName },
            { typeof(UpdateQaDashboardQueueMessage), QueueNames.UpdateQaDashboardQueueName },
            { typeof(VacancyAnalyticsQueueMessage), QueueNames.GenerateVacancyAnalyticsQueueName },
            { typeof(VacancyStatusQueueMessage), QueueNames.VacancyStatusQueueName },
            { typeof(UpdateEmployerUserAccountQueueMessage), QueueNames.UpdateEmployerUserAccountQueueName }
        };

        private readonly string _connectionString;

        public StorageQueueService(StorageQueueConnectionDetails details)
        {
            _connectionString = details.ConnectionString;
        }

        public async Task AddMessageAsync<T>(T message)
        {
            var queueName = _messageToStorageQueueMapper[typeof(T)];

            if(string.IsNullOrEmpty(queueName))
                throw new InvalidEnumArgumentException($"Cannot map type {typeof(T).Name} to a queue name");

            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var client = storageAccount.CreateCloudQueueClient();

            var queue = client.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();

            var cloudMessage = new CloudQueueMessage(JsonConvert.SerializeObject(message, Formatting.Indented));

            await queue.AddMessageAsync(cloudMessage);
        }
    }
}
