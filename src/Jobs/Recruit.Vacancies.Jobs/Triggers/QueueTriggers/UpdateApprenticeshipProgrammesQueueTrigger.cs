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
    public class UpdateApprenticeshipProgrammesQueueTrigger
    {
        private readonly ILogger<UpdateApprenticeshipProgrammesQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IJobsVacancyClient _client;

        private string JobName => GetType().Name;

        public UpdateApprenticeshipProgrammesQueueTrigger(ILogger<UpdateApprenticeshipProgrammesQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IJobsVacancyClient client)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _client = client;
        }

        public async Task UpdateApprenticeshipProgrammesAsync([QueueTrigger(QueueNames.UpdateApprenticeProgrammesQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting populating standards and frameworks into reference data store.");

            try
            {
                await _client.UpdateApprenticeshipProgrammesAsync();
                _logger.LogInformation("Finished populating standards and frameworks into reference data store.");

                await _client.UpdateApprenticeshipRouteAsync();
                _logger.LogInformation("Finished populating routes into reference data store.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update standards and frameworks.");
                throw;
            }
        }
    }
}