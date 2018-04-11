using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Recruit.Vacancies.Client.Infrastructure.Events;

namespace Esfa.Recruit.Vacancies.Jobs.GenerateVacancyNumber
{
    public class VacancyEventsJob
    {
        private readonly ILogger<VacancyEventsJob> _logger;
        private readonly GenerateVacancyNumberUpdater _updater;

        private string JobName => GetType().Name;

        public VacancyEventsJob(ILogger<VacancyEventsJob> logger, GenerateVacancyNumberUpdater updater)
        {
            _logger = logger;
            _updater = updater;
        }

        public async Task HandleVacancyEvent([QueueTrigger(QueueNames.VacancyEventsQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            IVacancyEvent @event = null;

            try
            {
                var eventItem = JsonConvert.DeserializeObject<EventItem>(message);

                @event = UnPackEvent(eventItem.EventType, eventItem.Data);

                _logger.LogInformation($"Start {JobName} For {{VacancyId}}", @event.VacancyId);

                await _updater.AssignVacancyNumber(@event.VacancyId);
                
                _logger.LogInformation($"Finished {JobName} For {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName} For {{VacancyId}}", @event?.VacancyId.ToString() ?? "unknown");
            }
        }

        private IVacancyEvent UnPackEvent(string eventType, string data)
        {
            switch (eventType)
            {
                case nameof(VacancyCreatedEvent):
                    return JsonConvert.DeserializeObject<VacancyCreatedEvent>(data);
                case nameof(VacancyUpdatedEvent):
                    return JsonConvert.DeserializeObject<VacancyUpdatedEvent>(data);
                case nameof(VacancySubmittedEvent):
                    return JsonConvert.DeserializeObject<VacancySubmittedEvent>(data);
                case nameof(VacancyDeletedEvent):
                    return JsonConvert.DeserializeObject<VacancyDeletedEvent>(data);
                default: 
                    throw new ArgumentOutOfRangeException(nameof(eventType), $"Unexpected value for event type: {eventType}");
            }
        }
    }
}

