using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.QueueTriggers
{
    public class GenerateAllEmployerDashboardsQueueTrigger
    {
        private readonly ILogger<GenerateAllEmployerDashboardsQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IEmployerDashboardProjectionService _projectionService;
        private string JobName => GetType().Name;

        public GenerateAllEmployerDashboardsQueueTrigger(ILogger<GenerateAllEmployerDashboardsQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IEmployerDashboardProjectionService projectionService)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _projectionService = projectionService;
        }

        public async Task GenerateAllEmployerDashboardsAsync([QueueTrigger(QueueNames.GenerateAllEmployerDashboardQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            const string individualJobName = "MultiEmployerDashboardGeneratorJob";
            if (_jobsConfig.DisabledJobs.Contains(individualJobName))
            {
                _logger.LogDebug($"{individualJobName} is disabled, skipping ...");
                return;
            }

            try
            {
                _logger.LogInformation($"Start {JobName}");

                await _projectionService.ReBuildAllDashboardsAsync();

                _logger.LogInformation($"Finished {JobName}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to deserialise event: {eventBody}", message);
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