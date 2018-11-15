using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.BankHoliday
{
    public class BankHolidayJob
    {
        private readonly ILogger<BankHolidayJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IJobsVacancyClient _client;

        public BankHolidayJob(ILogger<BankHolidayJob> logger, RecruitWebJobsSystemConfiguration jobsConfig, IJobsVacancyClient client)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _client = client;
        }

        public async Task UpdateBankHolidays([TimerTrigger(Schedules.MidnightDaily, RunOnStartup = true)]
            TimerInfo timerInfo, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(this.GetType().Name))
            {
                _logger.LogDebug($"{this.GetType().Name} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting updating Bank Holidays ReferenceData");

            await _client.UpdateBankHolidaysAsync();

            _logger.LogInformation("Finished updating Bank Holidays ReferenceData");
        }
    }
}

