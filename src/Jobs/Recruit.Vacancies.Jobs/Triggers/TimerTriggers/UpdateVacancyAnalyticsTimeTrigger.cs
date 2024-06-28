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

        var vacancies = await _analyticsAggregator.GetVacanciesWithAnalyticsInThePastHour();
        foreach (long vacancy in vacancies)
        {
            await _queue.AddMessageAsync(new VacancyAnalyticsV2QueueMessage
            {
                VacancyReference = vacancy
            });
        }
    }
}