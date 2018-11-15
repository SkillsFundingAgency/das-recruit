using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.QaDashboard
{
    public class QaDashboardJob
    {
        private readonly ILogger<QaDashboardJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IQaDashboardProjectionService _projectionService;

        public QaDashboardJob(ILogger<QaDashboardJob> logger, RecruitWebJobsSystemConfiguration jobsConfig, IQaDashboardProjectionService projectionService)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _projectionService = projectionService;
        }

        public async Task RebuildQaDashboard([TimerTrigger(Schedules.EveryFiveMinutes, RunOnStartup = true)]
            TimerInfo timerInfo, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(this.GetType().Name))
            {
                _logger.LogDebug($"{this.GetType().Name} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting rebuilding QA Dashboard");

            await _projectionService.RebuildQaDashboardAsync();

            _logger.LogInformation("Finished rebuilding QA Dashboard");
        }
    }
}
