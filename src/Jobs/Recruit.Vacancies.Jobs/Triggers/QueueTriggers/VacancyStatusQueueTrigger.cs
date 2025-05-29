using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class VacancyStatusQueueTrigger
    {
        private readonly ILogger<VacancyStatusQueueTrigger> _logger;
        private readonly IJobsVacancyClient _client;

        public VacancyStatusQueueTrigger(ILogger<VacancyStatusQueueTrigger> logger, IJobsVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task VacancyStatusAsync([QueueTrigger(QueueNames.VacancyStatusQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            _logger.LogInformation("Starting vacancy status checking.");

            try
            {
                await _client.CloseExpiredVacancies();
                _logger.LogInformation("Finished vacancy status checking.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to check vacancy statuses.");
                throw;
            }
        }
    }
}