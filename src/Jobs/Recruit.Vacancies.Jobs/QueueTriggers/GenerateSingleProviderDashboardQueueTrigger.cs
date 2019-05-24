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

namespace Esfa.Recruit.Vacancies.Jobs.QueueTriggers
{
    public class GenerateSingleProviderDashboardQueueTrigger
    {
        private readonly ILogger<GenerateSingleProviderDashboardQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IProviderDashboardProjectionService _projectionService;
        private string JobName => GetType().Name;

        public GenerateSingleProviderDashboardQueueTrigger(ILogger<GenerateSingleProviderDashboardQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IProviderDashboardProjectionService projectionService)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _projectionService = projectionService;
        }

        public async Task GenerateSingleProviderDashboardAsync([QueueTrigger(QueueNames.GenerateSingleProviderDashboardQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            const string individualJobName = "SingleProviderDashboardGeneratorJob";
            if (_jobsConfig.DisabledJobs.Contains(individualJobName))
            {
                _logger.LogDebug($"{individualJobName} is disabled, skipping ...");
                return;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<CreateProviderDashboardQueueMessage>(message);
                _logger.LogInformation($"Start {JobName} For Provider Account: {data.Ukprn}");

                await _projectionService.ReBuildDashboardAsync(data.Ukprn);

                _logger.LogInformation($"Finished {JobName} For Provider Account: {data.Ukprn}");
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