using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.DashboardGenerator
{
    public class DashboardGeneratorJob
    {
        private readonly ILogger<DashboardGeneratorJob> _logger;
        private readonly DashboardCreator _job;
        private string JobName => GetType().Name;

        public DashboardGeneratorJob(ILogger<DashboardGeneratorJob> logger, DashboardCreator job)
        {
            _logger = logger;
            _job = job;
        }

        public async Task GenerateEmployerVacancyData([QueueTrigger(QueueNames.DashboardQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<DashboardCreateMessage>(message);
                _logger.LogInformation($"Start {JobName} For Employer Account: {data.EmployerAccountId}");
                await _job.RunAsync(data.EmployerAccountId);
                _logger.LogInformation($"Finished {JobName} For Employer Account: {data.EmployerAccountId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
            }
        }

        class DashboardCreateMessage
        {
            public string EmployerAccountId { get; set; }
        }
    }
}