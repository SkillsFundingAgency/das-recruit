using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammesJob
    {
        private readonly ILogger<ApprenticeshipProgrammesJob> _logger;
        private readonly IJobsVacancyClient _client;

        public ApprenticeshipProgrammesJob(ILogger<ApprenticeshipProgrammesJob> logger, IJobsVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task UpdateStandardsAndFrameworks([TimerTrigger(Schedules.FourAmDaily, RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Starting populating standards and frameworks into Query Store");

            try
            {
                await _client.UpdateApprenticeshipProgrammesAsync();
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