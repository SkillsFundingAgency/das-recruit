using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.QueueTriggers
{
    public class GenerateVacancyAnalyticsQueueTrigger
    {
        private readonly ILogger<GenerateVacancyAnalyticsQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly AnalyticsEventStore _analyticsStore;
        private readonly IQueryStoreWriter _qsWriter;

        public GenerateVacancyAnalyticsQueueTrigger(ILogger<GenerateVacancyAnalyticsQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig,
            AnalyticsEventStore analyticsStore, IQueryStoreWriter qsWriter)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _analyticsStore = analyticsStore;
            _qsWriter = qsWriter;
        }

        public async Task GenerateVacancyAnalyticsAsync([QueueTrigger(QueueNames.GenerateVacancyAnalyticsQueueName, Connection = "QueueStorage")]
            string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(this.GetType().Name))
            {
                _logger.LogDebug($"{this.GetType().Name} is disabled, skipping ...");
                return;
            }

            var eventItem = JsonConvert.DeserializeObject<VacancyAnalyticsQueueMessage>(message);

            _logger.LogInformation($"Starting populating new vacancy analytics summary for vacancy reference {eventItem.VacancyReference} into query store.");

            try
            {
                var vacancyAnalyticSummary = await _analyticsStore.GetVacancyAnalyticEventSummaryAsync(eventItem.VacancyReference);

                await _qsWriter.UpsertVacancyAnalyticSummaryAsync(vacancyAnalyticSummary);

                _logger.LogInformation("Successfully finished populating new vacancy analytics summary for vacancy reference {eventItem.VacancyReference} into query store.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to populate new vacancy analytics summaries.");
                throw;
            }
        }
    }
}