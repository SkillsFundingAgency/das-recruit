using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.TimerTriggers
{
    public class UpdateApprenticeshipProgrammesTimerTrigger
    {
        private readonly ILogger<UpdateApprenticeshipProgrammesTimerTrigger> _logger;
        private readonly IQueue _queue;
        private readonly ITimeProvider _timeProvider;
        
        public UpdateApprenticeshipProgrammesTimerTrigger(ILogger<UpdateApprenticeshipProgrammesTimerTrigger> logger, IQueue queue, ITimeProvider timeProvider)
        {
            _logger = logger;
            _queue = queue;
            _timeProvider = timeProvider;
        }

        public Task UpdateApprenticeshipProgrammesAsync([TimerTrigger(Schedules.FourAmDaily)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation($"Timer trigger {this.GetType().Name} fired");

            var message = new UpdateApprenticeshipProgrammesQueueMessage
            {
                CreatedByScheduleDate = _timeProvider.Now
            };

            return _queue.AddMessageAsync(message);
        }
    }
}
