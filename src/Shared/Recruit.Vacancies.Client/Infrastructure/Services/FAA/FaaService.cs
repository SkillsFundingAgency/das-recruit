using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA
{
    public class FaaService : IFaaService
    {
        private const string ApplicationStatusSummaryTopicName = "UpdateApprenticeshipApplicationStatus";
        private const string UpdateApprenticeshipVacancyStatus = "UpdateApprenticeshipVacancyStatus";
		
        private readonly FaaConfiguration _config;

        public FaaService(IOptions<FaaConfiguration> config)
        {
            _config = config.Value;
        }

        public Task PublishApplicationStatusSummaryAsync(FaaApplicationStatusSummary message)
        {
            ServiceBusSender topicClient = CreateTopicClient(ApplicationStatusSummaryTopicName);

            var brokeredMessage = CreateMessage(message);

            return topicClient.SendMessageAsync(brokeredMessage);
        }

        public Task PublishVacancyStatusSummaryAsync(FaaVacancyStatusSummary message)
        {
            ServiceBusSender topicClient = CreateTopicClient(UpdateApprenticeshipVacancyStatus);
            var brokeredMessage = CreateMessage(message);
            return topicClient.SendMessageAsync(brokeredMessage);
        }

        private static ServiceBusMessage CreateMessage<T>(T messageObject) where T : class
        {
            string json = JsonConvert.SerializeObject(messageObject);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(json))
            {
                ContentType = "application/json"
            };
            return message;
        }

        private TopicClient CreateTopicClient(string topicName)
        {
            return new TopicClient(_config.StorageConnectionString, topicName, RetryPolicy.Default);
        }
    }
}

