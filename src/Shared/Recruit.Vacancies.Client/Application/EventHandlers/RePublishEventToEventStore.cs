using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class RePublishEventToEventStore : INotificationHandler<VacancyCreatedEvent>,
                                                   INotificationHandler<VacancyUpdatedEvent>,
                                                   INotificationHandler<VacancySubmittedEvent>
    {
        private readonly IEventStore _eventStore;

        public RePublishEventToEventStore(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken)
        {
            return HandleUsingEventStore(notification);
        }

        public Task Handle(VacancySubmittedEvent notification, CancellationToken cancellationToken)
        {
            return HandleUsingEventStore(notification);
        }

        public Task Handle(VacancyUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return HandleUsingEventStore(notification);
        }

        private async Task HandleUsingEventStore(IEvent @event)
        {
            await _eventStore.Add(@event);
        }

        
    }
}
