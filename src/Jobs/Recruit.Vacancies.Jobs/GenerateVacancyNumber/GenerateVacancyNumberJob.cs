using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.GenerateVacancyNumber
{
    public class GenerateVacancyNumberJob
    {
        private readonly ILogger<GenerateVacancyNumberJob> _logger;
        private string JobName => GetType().Name;

        public GenerateVacancyNumberJob(ILogger<GenerateVacancyNumberJob> logger)
        {
            _logger = logger;
        }

        public async Task GenerateVacancyNumber([QueueTrigger("vacancy-created-queue", Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                var eventItem = JsonConvert.DeserializeObject<EventItem>(message);
                var data = JsonConvert.DeserializeObject<VacancyCreatedEvent>(eventItem.Data);
                
                _logger.LogInformation($"Start {JobName} For ????");

                await Task.CompletedTask;
                
                _logger.LogInformation($"Finished {JobName} For ????");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
            }
        }

    }
}

