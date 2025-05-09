using System;
using System.IO;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class CommunicationsHouseKeepingQueueTrigger
    {
        private readonly ILogger<CommunicationsHouseKeepingQueueTrigger> _logger;
        private readonly ITimeProvider _timeProvider;
        private readonly ICommunicationRepository _communicationRepository;
        private const int DefaultStaleByDays = 180;
        private string JobName => GetType().Name;

        public CommunicationsHouseKeepingQueueTrigger(ILogger<CommunicationsHouseKeepingQueueTrigger> logger,
          ITimeProvider timeProvider,
          ICommunicationRepository communicationRepository)
        {
            _logger = logger;
            _timeProvider = timeProvider;
            _communicationRepository = communicationRepository;
        }

        public async Task CommunicationsHouseKeepingAsync([QueueTrigger(QueueNames.CommunicationsHouseKeepingQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            try
            {
                var payload = JsonConvert.DeserializeObject<CommunicationsHouseKeepingQueueMessage>(message);

                var targetDate = payload?.CreatedByScheduleDate ?? _timeProvider.Today;
                
                var deleteCommunicationsMessagesBefore180Days = targetDate.AddDays((DefaultStaleByDays) * -1);                

                await _communicationRepository.HardDelete(deleteCommunicationsMessagesBefore180Days);

                _logger.LogInformation($"Deleted Communication Mesages created before 180 days {deleteCommunicationsMessagesBefore180Days}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run {JobName}");
                throw;
            }
        }

    }
}