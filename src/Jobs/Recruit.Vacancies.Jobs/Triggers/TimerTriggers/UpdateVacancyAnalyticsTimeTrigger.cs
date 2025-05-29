using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.TimerTriggers;

public class UpdateVacancyAnalyticsTimeTrigger
{
    private readonly ILogger<UpdateVacancyAnalyticsTimeTrigger> _logger;
    private readonly IRecruitQueueService _queue;
    private readonly IAnalyticsAggregator _analyticsAggregator;

    public UpdateVacancyAnalyticsTimeTrigger(ILogger<UpdateVacancyAnalyticsTimeTrigger> logger, IRecruitQueueService queue, IAnalyticsAggregator analyticsAggregator)
    {
        _logger = logger;
        _queue = queue;
        _analyticsAggregator = analyticsAggregator;
    }
        
    public async Task UpdateVacancyAnalyticsAsync([TimerTrigger(Schedules.Hourly)] TimerInfo timerInfo, TextWriter log)
    {
        _logger.LogInformation($"Timer trigger {this.GetType().Name} fired");

        var vacancyMetrics = await _analyticsAggregator.GetVacanciesWithAnalyticsInThePastHour();
        foreach (var vacancyMetric in vacancyMetrics)
        {
            await _queue.AddMessageAsync(new VacancyAnalyticsV2QueueMessage
            {
                VacancyReference = vacancyMetric.VacancyReference,
                ViewsCount = vacancyMetric.ViewsCount,
                ApplicationSubmittedCount = vacancyMetric.ApplicationSubmittedCount,
                SearchResultsCount = vacancyMetric.SearchResultsCount,
                ApplicationStartedCount = vacancyMetric.ApplicationStartedCount
            });
        }
    }
}