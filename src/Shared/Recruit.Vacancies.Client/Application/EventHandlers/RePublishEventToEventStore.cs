using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class RePublishEventToEventStore : INotificationHandler<VacancyCreatedEvent>,
                                                   INotificationHandler<VacancyDraftUpdatedEvent>,
                                                   INotificationHandler<VacancySubmittedEvent>,
                                                   INotificationHandler<VacancyReviewApprovedEvent>,
                                                   INotificationHandler<VacancyReviewReferredEvent>
    {
        private readonly IEventStore _eventStore;

        public RePublishEventToEventStore(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken) => HandleUsingEventStore(notification);

        public Task Handle(VacancySubmittedEvent notification, CancellationToken cancellationToken) => HandleUsingEventStore(notification);

        public Task Handle(VacancyDraftUpdatedEvent notification, CancellationToken cancellationToken) => HandleUsingEventStore(notification);

        public Task Handle(VacancyReviewApprovedEvent notification, CancellationToken cancellationToken) => HandleUsingEventStore(notification);

        public Task Handle(VacancyReviewReferredEvent notification, CancellationToken cancellationToken) => HandleUsingEventStore(notification);

        private async Task HandleUsingEventStore(IEvent @event)
        {
            await _eventStore.Add(@event);
        }
    }
}
