using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.EditVacancyInfo
{
    public class EditVacancyInfoJob
    {
        private readonly ILogger<EditVacancyInfoJob> _logger;
        private readonly EditVacancyInfoUpdater _job;
        private string JobName => GetType().Name;

        public EditVacancyInfoJob(ILogger<EditVacancyInfoJob> logger, EditVacancyInfoUpdater job)
        {
            _logger = logger;
            _job = job;
        }

        public async Task GenerateEmployerVacancyData([QueueTrigger("user-signed-in-queue", Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                var eventItem = JsonConvert.DeserializeObject<EventItem>(message);
                var data = JsonConvert.DeserializeObject<UserSignedInEvent>(eventItem.Data);
                _logger.LogInformation($"Start {JobName} For Employer Account: {data.EmployerAccountId}");
                await _job.UpdateEditVacancyInfo(data.EmployerAccountId);
                _logger.LogInformation($"Finished {JobName} For Employer Account: {data.EmployerAccountId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
            }
        }
    }
}