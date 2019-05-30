using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.TimerTriggers
{
    public class GenerateBlockedEmployersTimeTrigger
    {
        private readonly ILogger<GenerateBlockedEmployersTimeTrigger> _logger;
        private readonly IQueueService _queue;
        private readonly ITimeProvider _timeProvider;

        public GenerateBlockedEmployersTimeTrigger(ILogger<GenerateBlockedEmployersTimeTrigger> logger, IQueueService queue, ITimeProvider timeProvider)
        {
            _logger = logger;
            _queue = queue;
            _timeProvider = timeProvider;
        }

        public Task UpdateBlockedEmployersAsync([TimerTrigger(Schedules.Hourly)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation($"Timer trigger {this.GetType().Name} fired");

            var message = new UpdateBlockedEmployersQueueMessage 
            {
                CreatedByScheduleDate = _timeProvider.Now
            };

            return _queue.AddMessageAsync(message);
        }
    }
}
