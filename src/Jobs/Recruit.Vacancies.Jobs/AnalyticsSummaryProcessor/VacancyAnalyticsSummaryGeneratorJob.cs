using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Jobs;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor
{
    public class VacancyAnalyticsSummaryGeneratorJob
    {
        private readonly ILogger<VacancyAnalyticsSummaryGeneratorJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly AnalyticsEventStore _analyticsStore;
        private readonly IQueryStoreWriter _qsWriter;

        public VacancyAnalyticsSummaryGeneratorJob(ILogger<VacancyAnalyticsSummaryGeneratorJob> logger, RecruitWebJobsSystemConfiguration jobsConfig,
                                                AnalyticsEventStore analyticsStore, IQueryStoreWriter qsWriter)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _analyticsStore = analyticsStore;
            _qsWriter = qsWriter;
        }

        public async Task ProcessEvents([TimerTrigger(Schedules.EveryFifteenMinutes, RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(this.GetType().Name))
            {
                _logger.LogDebug($"{this.GetType().Name} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting populating new vacancy analytics summaries into query store.");

            try
            {
                var timer = Stopwatch.StartNew();

                var eventSummariesAndLastEventId = await _analyticsStore.GetVacancyAnalyticEventSummariesAsync();
                var eventSummaries = eventSummariesAndLastEventId.Item1;
                var lastProcessedVacancyEventId = eventSummariesAndLastEventId.Item2;

                _logger.LogInformation($"Found {eventSummaries.Count} summaries to populate into the queryStore. Took {timer.Elapsed:c}.");

                if (eventSummaries.Count > 0)
                {
                    var tsUpsertStart = timer.Elapsed;
                    await _qsWriter.UpsertVacancyAnalyticSummaries(eventSummaries);

                    _logger.LogInformation($"Upserting {eventSummaries.Count} vacancy analytic summaries took {timer.Elapsed.Subtract(tsUpsertStart):c}");

                    await _analyticsStore.UpdateLastProcessedVacancyEventIdAsync(lastProcessedVacancyEventId);
                    _logger.LogInformation("Successfully finished populating new vacancy analytics summaries into query store.");
                }

                timer.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to populate new vacancy analytics summaries.");
                throw;
            }
            
            _logger.LogInformation($"Time to next run is {timerInfo.ScheduleStatus.Next.Subtract(DateTime.Now).Duration():c} away/overdue.");
        }
    }
}