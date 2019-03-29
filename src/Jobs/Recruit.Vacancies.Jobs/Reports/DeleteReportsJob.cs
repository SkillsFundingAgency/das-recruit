using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Reports
{
    public class DeleteReportsJob
    {
        private const int DeleteReportAfterTimeSpanDays = 7;

        private readonly ILogger<DeleteReportsJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly ITimeProvider _timeProvider;
        private readonly IReportRepository _reportRepository;

        private string JobName => GetType().Name;

        public DeleteReportsJob(ILogger<DeleteReportsJob> logger, 
            RecruitWebJobsSystemConfiguration jobsConfig,
            ITimeProvider timeProvider,
            IReportRepository reportRepository)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _timeProvider = timeProvider;
            _reportRepository = reportRepository;
        }

        public async Task DeleteReports([TimerTrigger(Schedules.MidnightDaily, RunOnStartup = true)]
            TimerInfo timerInfo, TextWriter log)
        {
            try
            {
                if (_jobsConfig.DisabledJobs.Contains(JobName))
                {
                    _logger.LogDebug($"{JobName} is disabled, skipping ...");
                    return;
                }

                var deleteReportsCreatedBeforeDate = _timeProvider.Today.AddDays(DeleteReportAfterTimeSpanDays * -1);

                var deletedCount = await _reportRepository.DeleteReportsCreatedBeforeAsync(deleteReportsCreatedBeforeDate);

                _logger.LogInformation($"Deleted {deletedCount} reports created before {deleteReportsCreatedBeforeDate}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run {JobName}");
                throw;
            }
        }
    }
}
