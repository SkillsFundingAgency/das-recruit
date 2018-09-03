using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.LiveVacanciesGenerator
{
    public class LiveVacanciesGeneratorJob
    {
        private readonly ILogger<LiveVacanciesGeneratorJob> _logger;
        private readonly ILiveVacancyProjectionService _projectionService;
        private string JobName => GetType().Name;

        public LiveVacanciesGeneratorJob(ILogger<LiveVacanciesGeneratorJob> logger, ILiveVacancyProjectionService projectionService)
        {
            _logger = logger;
            _projectionService = projectionService;
        }

        public async Task GenerateLiveVacanciesProjectionsAsync([QueueTrigger(QueueNames.GenerateLiveVacanciesQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    _logger.LogInformation($"Start {JobName}");

                    await _projectionService.ReGenerateLiveVacanciesAsync();
                    
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