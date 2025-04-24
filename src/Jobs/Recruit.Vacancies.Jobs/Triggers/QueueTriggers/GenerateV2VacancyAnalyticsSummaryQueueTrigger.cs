using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;

public class GenerateV2VacancyAnalyticsSummaryQueueTrigger
{
    private readonly ILogger<GenerateV2VacancyAnalyticsSummaryQueueTrigger> _logger;
    private readonly IAnalyticsAggregator _analyticsAggregator;
    private readonly IQueryStoreWriter _qsWriter;

    public GenerateV2VacancyAnalyticsSummaryQueueTrigger(ILogger<GenerateV2VacancyAnalyticsSummaryQueueTrigger> logger,
        IQueryStoreWriter qsWriter,IAnalyticsAggregator analyticsAggregator)
    {
        _logger = logger;
        _qsWriter = qsWriter;
        _analyticsAggregator = analyticsAggregator;
    }

    public async Task GenerateVacancyAnalyticsSummaryAsync([QueueTrigger(QueueNames.GenerateV2VacancyAnalyticsQueueName, Connection = "QueueStorage")]
        string message)
    {
        var eventItem = JsonConvert.DeserializeObject<VacancyAnalyticsV2QueueMessage>(message);

        _logger.LogInformation("Starting populating V2 vacancy analytics summary for vacancy reference {eventItem.VacancyReference} into query store.");

        try
        {
            var vacancyAnalyticSummary = await _analyticsAggregator.GetVacancyAnalyticEventSummaryAsync(eventItem);

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