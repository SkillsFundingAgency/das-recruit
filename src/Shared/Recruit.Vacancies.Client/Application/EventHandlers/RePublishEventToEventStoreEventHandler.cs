using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class RePublishEventToEventStoreEventHandler(
        ILogger<RePublishEventToEventStoreEventHandler> logger,
        IEventStore eventStore)
        :
            INotificationHandler<DraftVacancyUpdatedEvent>,
            INotificationHandler<VacancyReviewApprovedEvent>,
            INotificationHandler<SetupEmployerEvent>,
            INotificationHandler<SetupProviderEvent>,
            INotificationHandler<VacancyClosedEvent>,
            INotificationHandler<LiveVacancyUpdatedEvent>
    {
        public Task Handle(DraftVacancyUpdatedEvent notification, CancellationToken cancellationToken)
            => HandleUsingEventStore(notification);

        public Task Handle(VacancyReviewApprovedEvent notification, CancellationToken cancellationToken)
            => HandleUsingEventStore(notification);


        public Task Handle(SetupEmployerEvent notification, CancellationToken cancellationToken)
            => HandleUsingEventStore(notification);

        public Task Handle(SetupProviderEvent notification, CancellationToken cancellationToken)
            => HandleUsingEventStore(notification);

        public Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
            => HandleUsingEventStore(notification);

        public Task Handle(LiveVacancyUpdatedEvent notification, CancellationToken cancellationToken)
            => HandleUsingEventStore(notification);
        private async Task HandleUsingEventStore(IEvent @event)
        {
            logger.LogInformation("Re-publishing event {eventType} to event store", @event.GetType().Name);

            await eventStore.Add(@event);
        }
    }
}
