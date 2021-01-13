using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class UpdateProvidersQueueTrigger
    {
        private readonly ILogger<UpdateProvidersQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IJobsVacancyClient _client;

        private string JobName => GetType().Name;

        public UpdateProvidersQueueTrigger(ILogger<UpdateProvidersQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IJobsVacancyClient client)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _client = client;
        }

        public async Task UpdateProviders([QueueTrigger(QueueNames.UpdateProvidersQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting populating providers into reference data store.");

            try
            {
                await _client.UpdateProviders();
                _logger.LogInformation("Finished populating providers into reference data store.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update providers.");
                throw;
            }
        }
    }
}