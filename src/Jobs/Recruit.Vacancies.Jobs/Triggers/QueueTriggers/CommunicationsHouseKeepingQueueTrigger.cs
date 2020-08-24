using System;
using System.IO;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class CommunicationsHouseKeepingQueueTrigger
    {
        private readonly ILogger<CommunicationsHouseKeepingQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly ITimeProvider _timeProvider;
        private readonly ICommunicationRepository _communicationRepository;

        private string JobName => GetType().Name;

        public CommunicationsHouseKeepingQueueTrigger(ILogger<CommunicationsHouseKeepingQueueTrigger> logger,
          RecruitWebJobsSystemConfiguration jobsConfig,
          ITimeProvider timeProvider,
          ICommunicationRepository communicationRepository)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _timeProvider = timeProvider;
            _communicationRepository = communicationRepository;
        }

        public async Task CommunicationsHouseKeepingAsync([QueueTrigger(QueueNames.CommunicationsHouseKeepingQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            try
            {
                if (_jobsConfig.DisabledJobs.Contains(JobName))
                {
                    _logger.LogDebug($"{JobName} is disabled, skipping ...");
                    return;
                }

                var payload = JsonConvert.DeserializeObject<CommunicationsHouseKeepingQueueMessage>(message);

                var targetDate = payload?.CreatedByScheduleDate ?? _timeProvider.Today;

                var deleteCommunicationsMessagesBefore180Days = targetDate.AddDays(-180);

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