using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class RePublishEventToEventStore : 
                                            INotificationHandler<VacancyCreatedEvent>,
                                            INotificationHandler<DraftVacancyUpdatedEvent>,
                                            INotificationHandler<VacancySubmittedEvent>,
                                            INotificationHandler<VacancyReviewApprovedEvent>,
                                            INotificationHandler<VacancyReviewReferredEvent>,
                                            INotificationHandler<SetupEmployerEvent>,
                                            INotificationHandler<VacancyReviewCreatedEvent>
    {
        private readonly ILogger<RePublishEventToEventStore> _logger;
        private readonly IEventStore _eventStore;

        public RePublishEventToEventStore(ILogger<RePublishEventToEventStore> logger, IEventStore eventStore)
        {
            _logger = logger;
            _eventStore = eventStore;
        }

        public Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken) 
            => HandleUsingEventStore(notification);

        public Task Handle(VacancySubmittedEvent notification, CancellationToken cancellationToken) 
            => HandleUsingEventStore(notification);

        public Task Handle(DraftVacancyUpdatedEvent notification, CancellationToken cancellationToken) 
            => HandleUsingEventStore(notification);

        public Task Handle(VacancyReviewApprovedEvent notification, CancellationToken cancellationToken) 
            => HandleUsingEventStore(notification);

        public Task Handle(VacancyReviewReferredEvent notification, CancellationToken cancellationToken) 
            => HandleUsingEventStore(notification);

        public Task Handle(SetupEmployerEvent notification, CancellationToken cancellationToken) 
            => HandleUsingEventStore(notification);

        public Task Handle(VacancyReviewCreatedEvent notification, CancellationToken cancellationToken) 
            => HandleUsingEventStore(notification);

        private async Task HandleUsingEventStore(IEvent @event)
        {
            _logger.LogInformation("Re-publishing event {eventType} to event store", @event.GetType().Name);
            
            await _eventStore.Add(@event);
        }
    }
}
