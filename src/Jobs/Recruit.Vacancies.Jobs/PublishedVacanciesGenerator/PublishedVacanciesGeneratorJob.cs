using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.PublishedVacanciesGenerator
{
    public class PublishedVacanciesGeneratorJob
    {
        private readonly ILogger<PublishedVacanciesGeneratorJob> _logger;
        private readonly IPublishedVacancyProjectionService _projectionService;
        private string JobName => GetType().Name;

        public PublishedVacanciesGeneratorJob(ILogger<PublishedVacanciesGeneratorJob> logger, IPublishedVacancyProjectionService projectionService)
        {
            _logger = logger;
            _projectionService = projectionService;
        }

        public async Task GeneratePublishedVacanciesProjectionsAsync([QueueTrigger(QueueNames.GeneratePublishedVacanciesQueueName, Connection = "QueueStorage")] string message, TextWriter log)
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