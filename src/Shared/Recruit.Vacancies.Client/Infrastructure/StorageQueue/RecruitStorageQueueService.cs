using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    internal class RecruitStorageQueueService : StorageQueueServiceBase, IRecruitQueueService
    {
        private readonly Dictionary<Type, string> _messageToStorageQueueMapper = new Dictionary<Type, string>
        {
            { typeof(DeleteReportsQueueMessage), QueueNames.DeleteReportsQueueName },
            { typeof(EventItem), QueueNames.DomainEventsQueueName },
            { typeof(ReportQueueMessage), QueueNames.ReportQueueName },
            { typeof(TransferVacanciesFromProviderQueueMessage), QueueNames.TransferVacanciesFromProviderQueueName },
            { typeof(TransferVacancyToLegalEntityQueueMessage), QueueNames.TransferVacanciesToLegalEntityQueueName },
            { typeof(UpdateBankHolidaysQueueMessage), QueueNames.UpdateBankHolidaysQueueName },
            { typeof(UpdateQaDashboardQueueMessage), QueueNames.UpdateQaDashboardQueueName },
            { typeof(VacancyAnalyticsQueueMessage), QueueNames.GenerateVacancyAnalyticsQueueName },
            { typeof(VacancyStatusQueueMessage), QueueNames.VacancyStatusQueueName },
            { typeof(UpdateEmployerUserAccountQueueMessage), QueueNames.UpdateEmployerUserAccountQueueName },
            { typeof(DeleteStaleQueryStoreDocumentsQueueMessage), QueueNames.DeleteStaleQueryStoreDocumentsQueueName },
            { typeof(CommunicationsHouseKeepingQueueMessage), QueueNames.CommunicationsHouseKeepingQueueName},
            { typeof(UpdateProvidersQueueMessage), QueueNames.UpdateProvidersQueueName},
            { typeof(UpdateProviderInfoQueueMessage), QueueNames.UpdateProviderInfoQueueName},
            { typeof(TransferVacanciesFromEmployerReviewToQAReviewQueueMessage), QueueNames.TransferVacanciesFromEmployerReviewToQAReviewQueueName },
            { typeof(VacancyAnalyticsV2QueueMessage), QueueNames.GenerateV2VacancyAnalyticsQueueName }
        };

        protected override string ConnectionString { get; }

        public RecruitStorageQueueService(string connString)
        {
            ConnectionString = connString;
        }

        public override async Task AddMessageAsync<T>(T message)
        {
            var queueName = _messageToStorageQueueMapper[typeof(T)];

            if(string.IsNullOrEmpty(queueName))
                throw new InvalidEnumArgumentException($"Cannot map type {typeof(T).Name} to a queue name");

            var queueClient = new QueueClient(ConnectionString, queueName);

            await queueClient.CreateIfNotExistsAsync();

            string messageText = JsonConvert.SerializeObject(message);
            await queueClient.SendMessageAsync(messageText);

        }
    }
}
