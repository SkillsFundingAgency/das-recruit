using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.EmployerVacancyDataGenerator
{
    public class EmployerVacancyDataGeneratorJob
    {
        private readonly ILogger<EmployerVacancyDataGeneratorJob> _logger;
        private readonly EmployerVacancyDataGenerator _job;
        private string JobName => GetType().Name;

        public EmployerVacancyDataGeneratorJob(ILogger<EmployerVacancyDataGeneratorJob> logger, EmployerVacancyDataGenerator job)
        {
            _logger = logger;
            _job = job;
        }

        public async Task GenerateEmployerVacancyData([QueueTrigger("events", Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                var eventItem = JsonConvert.DeserializeObject<EventItem>(message);
                var data = JsonConvert.DeserializeObject<UserSignedInEvent>(eventItem.Data);
                _logger.LogInformation($"Start {JobName} For Employer Account: {data.EmployerAccountId}");
                await _job.GenerateVacancyDataForEmployer(data.EmployerAccountId);
                _logger.LogInformation($"Finished {JobName} For Employer Account: {data.EmployerAccountId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
            }
        }
    }
}