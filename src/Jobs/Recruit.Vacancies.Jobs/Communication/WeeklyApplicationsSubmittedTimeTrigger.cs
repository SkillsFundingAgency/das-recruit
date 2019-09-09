using System;
using System.IO;
using System.Threading.Tasks;
using Communication.Types;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Communications;

namespace Esfa.Recruit.Vacancies.Jobs.Communication
{
    public class WeeklyApplicationsSubmittedTimeTrigger
    {
        private readonly ILogger<WeeklyApplicationsSubmittedTimeTrigger> _logger;
        private readonly ICommunicationQueueService _queue;
        private readonly ITimeProvider _timeProvider;

        public WeeklyApplicationsSubmittedTimeTrigger(ILogger<WeeklyApplicationsSubmittedTimeTrigger> logger, ICommunicationQueueService queue, ITimeProvider timeProvider)
        {
            _logger = logger;
            _queue = queue;
            _timeProvider = timeProvider;
        }

        public Task WeeklyApplicationsSubmittedCreateAggregateRequestAsync([TimerTrigger(Schedules.EightAmWeeklySunday)] TimerInfo timerInfo, TextWriter log)
        {
            const int eightAmHour = 8;
            _logger.LogInformation($"Timer trigger {this.GetType().Name} fired");

            var to = _timeProvider.Today.AddHours(eightAmHour);
            var from = to.AddDays(-7);

            var message = new AggregateCommunicationRequest(Guid.NewGuid(), CommunicationConstants.RequestType.ApplicationSubmitted, DeliveryFrequency.Daily, _timeProvider.Now, from, to);

            return _queue.AddMessageAsync(message);
        }
    }
}
