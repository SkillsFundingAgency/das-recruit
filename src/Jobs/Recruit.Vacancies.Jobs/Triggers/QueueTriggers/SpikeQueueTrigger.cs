using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

#if DEBUG
namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class SpikeQueueTrigger
    {
        private readonly ILogger<SpikeQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IVacancySummariesProvider _query;

        private string JobName => GetType().Name;

        public SpikeQueueTrigger(ILogger<SpikeQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IVacancySummariesProvider query)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _query = query;
        }

        public async Task SpikeAsync([QueueTrigger("test-queue", Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            var employerAccountId = Environment.GetEnvironmentVariable("EmployerAccount");
            var result = await _query.GetEmployerOwnedVacancySummariesByEmployerAccountAsync(employerAccountId);
        }
    }
}
#endif