using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Esfa.Recruit.Vacancies.Jobs.Jobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class TransferVacancyToLegalEntityQueueTrigger
    {
        private readonly ILogger<TransferVacancyToLegalEntityQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IJobsVacancyClient _client;
        private readonly TransferVacancyToLegalEntityJob _runner;

        private string TriggerName => GetType().Name;

        public TransferVacancyToLegalEntityQueueTrigger(ILogger<TransferVacancyToLegalEntityQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig,
                                                        IJobsVacancyClient client, TransferVacancyToLegalEntityJob runner)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _client = client;
            _runner = runner;
        }

        public async Task TransferVacancyToLegalEntityAsync([QueueTrigger(QueueNames.TransferVacanciesToLegalEntityQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(TriggerName))
            {
                _logger.LogDebug($"{TriggerName} is disabled, skipping ...");
                return;
            }

            if (!string.IsNullOrEmpty(message))
            {
                _logger.LogInformation($"Starting queueing vacancy to transfer.");

                try
                {
                    var queueMessage = JsonConvert.DeserializeObject<TransferVacancyToLegalEntityQueueMessage>(message);

                    await _runner.Run(queueMessage.VacancyReference, queueMessage.UserRef, queueMessage.UserEmailAddress, queueMessage.UserName, queueMessage.TransferReason);

                    _logger.LogInformation("Finished queuing vacancy to transfer.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to queue vacancy to transfer.");
                    throw;
                }
            }
        }
    }
}