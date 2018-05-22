using System;
using System.IO;
using System.Threading.Tasks;
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
        private readonly LiveVacancyStatusInspector _inspector;

        public VacancyStatusJob(ILogger<VacancyStatusJob> logger, LiveVacancyStatusInspector inspector)
        {
            _logger = logger;
            _inspector = inspector;
        }

        public async Task Run([TimerTrigger(Schedules.MidnightDaily, RunOnStartup = CanRunOnStartup)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Starting vacancy status checking.");

            try
            {
                await _inspector.InspectAsync();
                _logger.LogInformation("Finished vacancy status checking.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to check vacancy statuses.");
            }
        }
    }
}