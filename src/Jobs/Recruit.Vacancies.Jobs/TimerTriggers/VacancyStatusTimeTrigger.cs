using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.TimerTriggers
{
    public class VacancyStatusTimeTrigger
    {
        private readonly ILogger<VacancyStatusTimeTrigger> _logger;
        private readonly IQueue _queue;
        private readonly ITimeProvider _timeProvider;

        public VacancyStatusTimeTrigger(ILogger<VacancyStatusTimeTrigger> logger, IQueue queue, ITimeProvider timeProvider)
        {
            _logger = logger;
            _queue = queue;
            _timeProvider = timeProvider;
        }

        public Task VacancyStatusAsync([TimerTrigger(Schedules.MidnightDaily)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation($"Timer trigger {this.GetType().Name} fired");

            var message = new VacancyStatusQueueMessage
            {
                CreatedByScheduleDate = _timeProvider.Now
            };

            return _queue.AddMessageAsync(message);
        }
    }
}
