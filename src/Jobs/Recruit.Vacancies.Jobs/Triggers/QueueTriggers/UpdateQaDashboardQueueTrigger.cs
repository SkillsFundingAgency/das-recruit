using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class UpdateQaDashboardQueueTrigger
    {
        private readonly ILogger<UpdateQaDashboardQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IQaDashboardProjectionService _projectionService;

        private string JobName => GetType().Name;

        public UpdateQaDashboardQueueTrigger(ILogger<UpdateQaDashboardQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IQaDashboardProjectionService projectionService)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _projectionService = projectionService;
        }

        public async Task UpdateQaDashboardAsync([QueueTrigger(QueueNames.UpdateQaDashboardQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting rebuilding QA Dashboard");

            await _projectionService.RebuildQaDashboardAsync();

            _logger.LogInformation("Finished rebuilding QA Dashboard");
        }
    }
}
