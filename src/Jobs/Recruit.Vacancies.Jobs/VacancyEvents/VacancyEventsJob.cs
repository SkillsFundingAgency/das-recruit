using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyEvents
{
    public class VacancyEventsJob
    {
        private readonly ILogger<VacancyEventsJob> _logger;
        private readonly VacancyEventHandler _vacancyHandler;

        public VacancyEventsJob(ILogger<VacancyEventsJob> logger, VacancyEventHandler vacancyHandler)
        {
            _logger = logger;
            _vacancyHandler = vacancyHandler;
        }

        public async Task HandleVacancyEvent([QueueTrigger(QueueNames.VacancyEventsQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                var eventItem = JsonConvert.DeserializeObject<EventItem>(message);
                
                await UnpackAndExecute(eventItem.EventType, eventItem.Data);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to deserialise event: {eventBody}", message);
            }
        }

        private Task UnpackAndExecute(string eventType, string data)
        {
            switch (eventType)
            {
                case nameof(VacancyCreatedEvent):
                    return _vacancyHandler.Handle(JsonConvert.DeserializeObject<VacancyCreatedEvent>(data));
                case nameof(VacancyDraftUpdatedEvent):
                    return _vacancyHandler.Handle(JsonConvert.DeserializeObject<VacancyDraftUpdatedEvent>(data));
                case nameof(VacancySubmittedEvent):
                    return _vacancyHandler.Handle(JsonConvert.DeserializeObject<VacancySubmittedEvent>(data));
                default: 
                    throw new ArgumentOutOfRangeException(nameof(eventType), $"Unexpected value for event type: {eventType}");
            }
        }
    }
}

