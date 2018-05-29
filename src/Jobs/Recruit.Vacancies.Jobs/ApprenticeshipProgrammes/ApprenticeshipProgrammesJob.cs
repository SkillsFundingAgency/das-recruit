using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammesJob
    {
        private readonly ILogger<ApprenticeshipProgrammesJob> _logger;
        private ApprenticeshipProgrammesUpdater _updater;

        public ApprenticeshipProgrammesJob(ILogger<ApprenticeshipProgrammesJob> logger, ApprenticeshipProgrammesUpdater updater)
        {
            _logger = logger;
            _updater = updater;
        }

        public async Task UpdateStandardsAndFrameworks([TimerTrigger(Schedules.FourAmDaily, RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Starting populating standards and frameworks into Query Store");

            try
            {
                await _updater.UpdateAsync();
                _logger.LogInformation("Finished populating standards and frameworks into Query Store");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update standards and frameworks.");
                throw;
            }
        }
    }
}