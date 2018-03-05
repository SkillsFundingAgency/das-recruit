using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.TrainingTypes
{
    public class UpdateStandardsAndFrameworksJob
    {
        private readonly ILogger<UpdateStandardsAndFrameworksJob> _logger;
        private StandardsAndFrameworksUpdater _updater;

        public UpdateStandardsAndFrameworksJob(ILogger<UpdateStandardsAndFrameworksJob> logger, StandardsAndFrameworksUpdater updater)
        {
            _logger = logger;
            _updater = updater;
        }

        public async Task UpdateStandardsAndFrameworks([TimerTrigger("0 0 4 * * *", RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
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
            }
        }
    }
}