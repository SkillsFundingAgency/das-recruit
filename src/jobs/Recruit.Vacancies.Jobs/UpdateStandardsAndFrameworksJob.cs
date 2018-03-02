using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;

namespace Esfa.Recruit.Vacancies.Jobs
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

        public async Task UpdateStandardsAndFrameworks([TimerTrigger("0 0 2 * * *", RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Starting populating standards and frameworks into Query Store");

            try
            {
               await _updater.UpdateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get standards and frameworks from apprenticeship api.");
            }

            _logger.LogInformation("Finished populating standards and frameworks into Query Store");
        }
    }
}