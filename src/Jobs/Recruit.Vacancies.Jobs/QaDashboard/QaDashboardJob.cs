using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.QaDashboard
{
    public class QaDashboardJob
    {
        private readonly ILogger<QaDashboardJob> _logger;
        private readonly IQaDashboardProjectionService _projectionService;

        public QaDashboardJob(ILogger<QaDashboardJob> logger, IQaDashboardProjectionService qaDashboardService)
        {
            _logger = logger;
            _projectionService = qaDashboardService;
        }

        public async Task RebuildQaDashboard([TimerTrigger(Schedules.EveryFiveMinutes, RunOnStartup = true)]
            TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Starting rebuilding QA Dashboard");

            await _projectionService.RebuildQaDashboardAsync();

            _logger.LogInformation("Finished rebuilding QA Dashboard");
        }
    }
}
