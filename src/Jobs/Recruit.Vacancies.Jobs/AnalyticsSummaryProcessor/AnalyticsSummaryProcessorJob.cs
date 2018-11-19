using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Jobs;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfs.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor
{
    public class AnalyticsSummaryProcessorJob
    {
        private readonly ILogger<AnalyticsSummaryProcessorJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IJobsVacancyClient _client;

        public AnalyticsSummaryProcessorJob(ILogger<AnalyticsSummaryProcessorJob> logger, RecruitWebJobsSystemConfiguration jobsConfig, IJobsVacancyClient client)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _client = client;
        }

        public Task ProcessEvents([TimerTrigger(Schedules.EveryFifteenMinutes, RunOnStartup = false)] TimerInfo timerInfo, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(this.GetType().Name))
            {
                _logger.LogDebug($"{this.GetType().Name} is disabled, skipping ...");
                return;
            }
        }
    }
}