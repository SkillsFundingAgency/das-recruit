using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.QaDashboard
{
    public class QaDashboardJob
    {
        private readonly ILogger<QaDashboardJob> _logger;
        private readonly IQaDashboardService _qaDashboardService;

        public QaDashboardJob(ILogger<QaDashboardJob> logger, IQaDashboardService qaDashboardService)
        {
            _logger = logger;
            _qaDashboardService = qaDashboardService;
        }

        public async Task RebuildQaDashboard([TimerTrigger(Schedules.EveryFiveMinutes, RunOnStartup = true)]
            TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Starting rebuilding QA Dashboard");

            await _qaDashboardService.RebuildQaDashboardAsync();

            _logger.LogInformation("Finished rebuilding QA Dashboard");
        }
    }
}
