using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.GenerateVacancyNumber
{
    public class VacancyEventsJob
    {
        private readonly ILogger<VacancyEventsJob> _logger;
        private readonly VacancyEventHandler _handler;

        private string JobName => GetType().Name;

        public VacancyEventsJob(ILogger<VacancyEventsJob> logger, VacancyEventHandler handler)
        {
            _logger = logger;
            _handler = handler;
        }

        public async Task HandleVacancyEvent([QueueTrigger(QueueNames.VacancyEventsQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                var eventItem = JsonConvert.DeserializeObject<EventItem>(message);
                
                await UnpackAndExecute(eventItem.EventType, eventItem.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}");
            }
        }

        private Task UnpackAndExecute(string eventType, string data)
        {
            switch (eventType)
            {
                case nameof(VacancyCreatedEvent):
                    return _handler.Handle(JsonConvert.DeserializeObject<VacancyCreatedEvent>(data));
                case nameof(VacancyUpdatedEvent):
                    return _handler.Handle(JsonConvert.DeserializeObject<VacancyUpdatedEvent>(data));
                case nameof(VacancySubmittedEvent):
                    return _handler.Handle(JsonConvert.DeserializeObject<VacancySubmittedEvent>(data));
                case nameof(VacancyDeletedEvent):
                    return _handler.Handle(JsonConvert.DeserializeObject<VacancyDeletedEvent>(data));
                default: 
                    throw new ArgumentOutOfRangeException(nameof(eventType), $"Unexpected value for event type: {eventType}");
            }
        }
    }

}

