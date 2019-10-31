using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.TimerTriggers
{
    public class DeleteStaleVacanciesTimeTrigger
    {
        private readonly ILogger<DeleteStaleVacanciesTimeTrigger> _logger;
        private readonly IRecruitQueueService _queue;
        private readonly ITimeProvider _timeProvider;

        public DeleteStaleVacanciesTimeTrigger(ILogger<DeleteStaleVacanciesTimeTrigger> logger, IRecruitQueueService queue, ITimeProvider timeProvider)
        {
            _logger = logger;
            _queue = queue;
            _timeProvider = timeProvider;
        }

        public Task DeleteStaleVacanciesAsync([TimerTrigger(Schedules.WeeklySevenAmSunday)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation($"Timer trigger {this.GetType().Name} fired");

            var message = new DeleteStaleVacanciesQueueMessage
            {
                CreatedByScheduleDate = _timeProvider.Today
            };

            return _queue.AddMessageAsync(message);
        }
    }
}