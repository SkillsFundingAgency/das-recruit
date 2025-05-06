using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class GeneratePublishedVacanciesQueueTrigger
    {
        private readonly ILogger<GeneratePublishedVacanciesQueueTrigger> _logger;
        private readonly IPublishedVacancyProjectionService _projectionService;
        private string JobName => GetType().Name;

        public GeneratePublishedVacanciesQueueTrigger(ILogger<GeneratePublishedVacanciesQueueTrigger> logger, IPublishedVacancyProjectionService projectionService)
        {
            _logger = logger;
            _projectionService = projectionService;
        }

        public async Task GeneratePublishedVacanciesAsync([QueueTrigger(QueueNames.GeneratePublishedVacanciesQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
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