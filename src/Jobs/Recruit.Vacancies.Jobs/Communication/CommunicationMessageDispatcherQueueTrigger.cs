using System;
using System.IO;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Communication
{
    public class CommunicationMessageDispatcherQueueTrigger
    {
        private readonly ILogger<CommunicationMessageDispatcherQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly CommunicationMessageDispatcher _messageDispatcher;
        private string JobName => GetType().Name;

        public CommunicationMessageDispatcherQueueTrigger(ILogger<CommunicationMessageDispatcherQueueTrigger> logger,
            RecruitWebJobsSystemConfiguration jobsConfig, CommunicationMessageDispatcher messageDispatcher)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _messageDispatcher = messageDispatcher;
        }

        public async Task ProcessCommunicationRequestAsync([QueueTrigger(CommunicationQueueNames.CommunicationMessageDispatcher, Connection = "CommunicationsStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            try
            {
                var commMsgId = JsonConvert.DeserializeObject<CommunicationMessageIdentifier>(message);

                _logger.LogInformation($"Start {JobName} For Communication Message: {commMsgId.Id}");
                await _messageDispatcher.Run(commMsgId);
                _logger.LogInformation($"Finished {JobName} Communication Message: {commMsgId.Id}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to deserialize event: {eventBody}", message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
                throw;
            }
        }
    }
}