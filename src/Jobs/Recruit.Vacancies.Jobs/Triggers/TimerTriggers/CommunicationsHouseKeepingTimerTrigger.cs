using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.TimerTriggers
{
    public class CommunicationsHouseKeepingTimerTrigger
    {
        private readonly ILogger<CommunicationsHouseKeepingTimerTrigger> _logger;
        private readonly IRecruitQueueService _queue;
        private readonly ITimeProvider _timeProvider;

        public CommunicationsHouseKeepingTimerTrigger(ILogger<CommunicationsHouseKeepingTimerTrigger> logger, IRecruitQueueService queue, ITimeProvider timeProvider)
        {
            _logger = logger;
            _queue = queue;
            _timeProvider = timeProvider;
        }

        public Task CommunicationsHouseKeepingAsync([TimerTrigger(Schedules.WeeklyTenAmSunday)] TimerInfo timerInfo, TextWriter log)        
        {
            _logger.LogInformation($"Timer trigger {this.GetType().Name} fired");

            var message = new CommunicationsHouseKeepingQueueMessage
            {
                CreatedByScheduleDate = _timeProvider.Now
            };

            return _queue.AddMessageAsync(message);
        }
    }
}
