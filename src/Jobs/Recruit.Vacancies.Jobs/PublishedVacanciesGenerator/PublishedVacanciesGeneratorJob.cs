using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.PublishedVacanciesGenerator
{
    public class PublishedVacanciesGeneratorJob
    {
        private readonly ILogger<PublishedVacanciesGeneratorJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IPublishedVacancyProjectionService _projectionService;
        private string JobName => GetType().Name;

        public PublishedVacanciesGeneratorJob(ILogger<PublishedVacanciesGeneratorJob> logger, RecruitWebJobsSystemConfiguration jobsConfig, IPublishedVacancyProjectionService projectionService)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _projectionService = projectionService;
        }

        public async Task GeneratePublishedVacanciesProjectionsAsync([QueueTrigger(QueueNames.GeneratePublishedVacanciesQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(this.GetType().Name))
            {
                _logger.LogDebug($"{this.GetType().Name} is disabled, skipping ...");
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    _logger.LogInformation($"Start {JobName}");

                    await _projectionService.ReGeneratePublishedVacanciesAsync();
                    
                    _logger.LogInformation($"Finished {JobName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
                throw;
            }
        }
    }
}