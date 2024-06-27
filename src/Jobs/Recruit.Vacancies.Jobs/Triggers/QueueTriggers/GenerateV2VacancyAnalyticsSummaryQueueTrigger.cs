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

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;

public class GenerateV2VacancyAnalyticsSummaryQueueTrigger
{
    private readonly ILogger<GenerateVacancyAnalyticsSummaryQueueTrigger> _logger;
    private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
    private readonly IAnalyticsAggregator _analyticsAggregator;
    private readonly IQueryStoreWriter _qsWriter;
        
    private string JobName => GetType().Name;

    public GenerateV2VacancyAnalyticsSummaryQueueTrigger(ILogger<GenerateVacancyAnalyticsSummaryQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig,
        IQueryStoreWriter qsWriter,IAnalyticsAggregator analyticsAggregator)
    {
        _logger = logger;
        _jobsConfig = jobsConfig;
        _qsWriter = qsWriter;
        _analyticsAggregator = analyticsAggregator;
    }

    public async Task GenerateVacancyAnalyticsSummaryAsync([QueueTrigger(QueueNames.GenerateV2VacancyAnalyticsQueueName, Connection = "QueueStorage")]
        string message, TextWriter log)
    {
        if (_jobsConfig.DisabledJobs.Contains(JobName))
        {
            _logger.LogDebug($"{JobName} is disabled, skipping ...");
            return;
        }

        var eventItem = JsonConvert.DeserializeObject<VacancyAnalyticsQueueMessage>(message);

        _logger.LogInformation($"Starting populating V2 vacancy analytics summary for vacancy reference {eventItem.VacancyReference} into query store.");

        try
        {
            var vacancyAnalyticSummary = await _analyticsAggregator.GetVacancyAnalyticEventSummaryAsync(eventItem.VacancyReference);

            await _qsWriter.UpsertVacancyAnalyticSummaryAsync(vacancyAnalyticSummary);

            _logger.LogInformation("Successfully finished populating V2 vacancy analytics summary for vacancy reference {eventItem.VacancyReference} into query store.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to populate new vacancy analytics summaries V2.");
            throw;
        }
    }
}