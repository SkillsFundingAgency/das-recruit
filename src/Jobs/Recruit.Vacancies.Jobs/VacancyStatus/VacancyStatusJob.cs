using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.ApprenticeshipProgrammes
{
    public class VacancyStatusJob
    {
        private readonly ILogger<VacancyStatusJob> _logger;
        private LiveVacancyStatusInspector _updater;

        public VacancyStatusJob(ILogger<VacancyStatusJob> logger, LiveVacancyStatusInspector updater)
        {
            _logger = logger;
            _updater = updater;
        }

#if DEBUG
        public async Task UpdateStandardsAndFrameworks([TimerTrigger(Schedules.Midnight, RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
#else
        public async Task UpdateStandardsAndFrameworks([TimerTrigger(Schedules.Midnight, RunOnStartup = false)] TimerInfo timerInfo, TextWriter log)
#endif
        {
            _logger.LogInformation("Starting vacancy status checking.");

            try
            {
                await _updater.InspectAsync();
                _logger.LogInformation("Finished vacancy status checking.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to check vacancy statuses.");
            }
        }
    }
}