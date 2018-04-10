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
        private readonly GenerateVacancyNumberUpdater _updater;

        private string JobName => GetType().Name;

        public GenerateVacancyNumberJob(ILogger<GenerateVacancyNumberJob> logger, GenerateVacancyNumberUpdater updater)
        {
            _logger = logger;
            _updater = updater;
        }

        public async Task GenerateVacancyNumber([QueueTrigger("vacancy-queue", Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            VacancyCreatedEvent data = null;

            try
            {
                var eventItem = JsonConvert.DeserializeObject<EventItem>(message);
                data = JsonConvert.DeserializeObject<VacancyCreatedEvent>(eventItem.Data);
                
                _logger.LogInformation($"Start {JobName} For {{VacancyId}}", data.VacancyId);

                await _updater.AssignVacancyNumber(data.VacancyId);
                
                _logger.LogInformation($"Finished {JobName} For {{VacancyId}}", data.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName} For {{VacancyId}}", data?.VacancyId.ToString() ?? "unknown");
            }
        }
    }
}

