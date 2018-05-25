using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyReviewEvents
{
    public class VacancyReviewEventsJob
    {
        private readonly ILogger<VacancyReviewEventsJob> _logger;
        private readonly VacancyReviewEventHandler _reviewHandler;

        public VacancyReviewEventsJob(ILogger<VacancyReviewEventsJob> logger, VacancyReviewEventHandler reviewHandler)
        {
            _logger = logger;
            _reviewHandler = reviewHandler;
        }

        public async Task HandleVacancyReviewEvent([QueueTrigger(QueueNames.VacancyReviewEventsQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                var eventItem = JsonConvert.DeserializeObject<EventItem>(message);
                
                await UnpackAndExecute(eventItem.EventType, eventItem.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to deserialise event: {eventBody}", message);
                throw;
            }
        }

        private Task UnpackAndExecute(string eventType, string data)
        {
            switch (eventType)
            {
                case nameof(VacancyReviewApprovedEvent):
                    return _reviewHandler.Handle(JsonConvert.DeserializeObject<VacancyReviewApprovedEvent>(data)); 
                case nameof(VacancyReviewReferredEvent):
                    return _reviewHandler.Handle(JsonConvert.DeserializeObject<VacancyReviewReferredEvent>(data)); 
                default: 
                    throw new ArgumentOutOfRangeException(nameof(eventType), $"Unexpected value for event type: {eventType}");
            }
        }
    }

}

