using System;
using System.IO;
using System.Threading.Tasks;
using Communication.Core;
using Communication.Types;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Communication
{
    public class CommunicationRequestQueueTrigger
    {
        private readonly ILogger<CommunicationRequestQueueTrigger> _logger;
        private readonly ICommunicationService _communicationService;

        private string JobName => GetType().Name;

        public CommunicationRequestQueueTrigger(ILogger<CommunicationRequestQueueTrigger> logger,
            ICommunicationService communicationService)
        {
            _logger = logger;
            _communicationService = communicationService;
        }

        public async Task ProcessCommunicationRequestAsync([QueueTrigger(CommunicationQueueNames.CommunicationRequests, Connection = "CommunicationsStorage")] string message, TextWriter log)
        {
            try
            {
                var commReq = JsonConvert.DeserializeObject<CommunicationRequest>(message);
                _logger.LogInformation($"Start {JobName} For Communication Request: {commReq.RequestType}:{commReq.RequestId}");

                await _communicationService.ProcessCommunicationRequestAsync(commReq);

                _logger.LogInformation($"Finished {JobName} For Communication Request: {commReq.RequestType}:{commReq.RequestId}");
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
