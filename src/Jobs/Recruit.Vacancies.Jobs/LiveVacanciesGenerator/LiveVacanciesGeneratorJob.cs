using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.LiveVacanciesGenerator
{
    public class LiveVacanciesGeneratorJob
    {
        private readonly ILogger<LiveVacanciesGeneratorJob> _logger;
        private readonly LiveVacanciesCreator _job;
        private string JobName => GetType().Name;

        public LiveVacanciesGeneratorJob(ILogger<LiveVacanciesGeneratorJob> logger, LiveVacanciesCreator job)
        {
            _logger = logger;
            _job = job;
        }

        public async Task GenerateLiveVacanciesProjectionsAsync([QueueTrigger(QueueNames.GenerateLiveVacanciesQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    _logger.LogInformation($"Start {JobName}");
                    await _job.RunAsync();
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