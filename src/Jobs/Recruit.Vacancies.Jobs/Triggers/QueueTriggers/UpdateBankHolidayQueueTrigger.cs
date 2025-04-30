using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class UpdateBankHolidayQueueTrigger
    {
        private readonly ILogger<UpdateBankHolidayQueueTrigger> _logger;
        private readonly IJobsVacancyClient _client;

        public UpdateBankHolidayQueueTrigger(ILogger<UpdateBankHolidayQueueTrigger> logger, IJobsVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task UpdateBankHolidaysAsync([QueueTrigger(QueueNames.UpdateBankHolidaysQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            _logger.LogInformation("Starting updating Bank Holidays ReferenceData");

            await _client.UpdateBankHolidaysAsync();

            _logger.LogInformation("Finished updating Bank Holidays ReferenceData");
        }
    }
}

