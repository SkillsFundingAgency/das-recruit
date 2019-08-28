using System;
using System.IO;
using System.Threading.Tasks;
using Communication.Core;
using Communication.Types;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Communication
{
    public class AggregateCommunicationComposeRequestQueueTrigger
    {
        private readonly ILogger<CommunicationRequestQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly ICommunicationService _communicationService;

        private string JobName => GetType().Name;

        public AggregateCommunicationComposeRequestQueueTrigger(ILogger<CommunicationRequestQueueTrigger> logger,
            RecruitWebJobsSystemConfiguration jobsConfig,
            ICommunicationService communicationService)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _communicationService = communicationService;
        }

        public async Task ProcessAggregateCommunicationComposeRequestAsync([QueueTrigger(CommunicationQueueNames.AggregateCommunicationComposeRequests, Connection = "CommunicationsStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            try
            {
                var req = JsonConvert.DeserializeObject<AggregateCommunicationComposeRequest>(message);
                _logger.LogInformation($"Start {JobName} For Aggregate Communication Compose Request: {req.AggregateCommunicationRequest.RequestType}:{req.UserId}");

                await _communicationService.ProcessAggregateCommunicationComposeRequestAsync(req);

                _logger.LogInformation($"Finished {JobName} For Aggregate Communication Compose Request: {req.AggregateCommunicationRequest.RequestType}:{req.UserId}");
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
