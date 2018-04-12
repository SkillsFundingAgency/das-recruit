using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class RePublishEventOnVacancyCreation : INotificationHandler<VacancyCreatedEvent>
    {
        private readonly IEventStore _eventStore;

        public RePublishEventOnVacancyCreation(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken)
        {
            await HandleUsingEventStore(notification);
        }

        private async Task HandleUsingEventStore(IEvent @event)
        {
            await _eventStore.Add(@event);
        }
    }
}
