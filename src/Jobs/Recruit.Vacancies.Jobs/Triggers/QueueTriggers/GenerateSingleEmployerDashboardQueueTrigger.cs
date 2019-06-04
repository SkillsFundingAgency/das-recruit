using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class GenerateSingleEmployerDashboardQueueTrigger
    {
        private readonly ILogger<GenerateSingleEmployerDashboardQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IEmployerDashboardProjectionService _projectionService;
        private string JobName => GetType().Name;

        public GenerateSingleEmployerDashboardQueueTrigger(ILogger<GenerateSingleEmployerDashboardQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IEmployerDashboardProjectionService projectionService)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _projectionService = projectionService;
        }

        public async Task GenerateSingleEmployerDashboardAsync([QueueTrigger(QueueNames.GenerateSingleEmployerDashboardQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<CreateEmployerDashboardQueueMessage>(message);
                _logger.LogInformation($"Start {JobName} For Employer Account: {data.EmployerAccountId}");

                await _projectionService.ReBuildDashboardAsync(data.EmployerAccountId);

                _logger.LogInformation($"Finished {JobName} For Employer Account: {data.EmployerAccountId}");
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