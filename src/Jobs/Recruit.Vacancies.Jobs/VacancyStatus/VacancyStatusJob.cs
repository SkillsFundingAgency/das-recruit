using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyStatus
{
    public class VacancyStatusJob
    {
#if DEBUG
        private const bool CanRunOnStartup = true;
#else
        private const bool CanRunOnStartup = false;
#endif
        private readonly ILogger<VacancyStatusJob> _logger;
        private readonly IJobsVacancyClient _client;

        public VacancyStatusJob(ILogger<VacancyStatusJob> logger, IJobsVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task Run([TimerTrigger(Schedules.MidnightDaily, RunOnStartup = CanRunOnStartup)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Starting vacancy status checking.");

            try
            {
                await _client.CloseExpiredVacancies();
                _logger.LogInformation("Finished vacancy status checking.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to check vacancy statuses.");
                throw;
            }
        }
    }
}