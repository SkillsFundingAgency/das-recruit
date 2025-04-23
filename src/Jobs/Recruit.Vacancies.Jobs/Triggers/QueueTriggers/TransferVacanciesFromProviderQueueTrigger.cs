using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Jobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class TransferVacanciesFromProviderQueueTrigger
    {
        private readonly ILogger<TransferVacanciesFromProviderQueueTrigger> _logger;
        private readonly TransferVacanciesFromProviderJob _runner;
        private string TriggerName => GetType().Name;

        public TransferVacanciesFromProviderQueueTrigger(ILogger<TransferVacanciesFromProviderQueueTrigger> logger, TransferVacanciesFromProviderJob runner)
        {
            _logger = logger;
            _runner = runner;
        }

        public async Task TransferVacanciesFromProvider([QueueTrigger(QueueNames.TransferVacanciesFromProviderQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (!string.IsNullOrEmpty(message))
            {
                _logger.LogInformation("Starting queueing vacancies to transfer from provider.");

                try
                {
                    var queueMessage = JsonConvert.DeserializeObject<TransferVacanciesFromProviderQueueMessage>(message);

                    await _runner.Run(queueMessage.Ukprn, queueMessage.EmployerAccountId, queueMessage.AccountLegalEntityPublicHashedId, queueMessage.UserRef, queueMessage.UserEmailAddress, queueMessage.UserName, queueMessage.TransferReason);

                    _logger.LogInformation("Finished queueing vacancies to transfer from provider.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unable to run {TriggerName}.");
                    throw;
                }
            }
        }
    }
}