using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class UpdateBankHolidayQueueTrigger
    {
        private readonly ILogger<UpdateBankHolidayQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IJobsVacancyClient _client;

        private string JobName => GetType().Name;

        public UpdateBankHolidayQueueTrigger(ILogger<UpdateBankHolidayQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IJobsVacancyClient client)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _client = client;
        }

        public async Task UpdateBankHolidaysAsync([QueueTrigger(QueueNames.UpdateBankHolidaysQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting updating Bank Holidays ReferenceData");

            await _client.UpdateBankHolidaysAsync();

            _logger.LogInformation("Finished updating Bank Holidays ReferenceData");
        }
    }
}

