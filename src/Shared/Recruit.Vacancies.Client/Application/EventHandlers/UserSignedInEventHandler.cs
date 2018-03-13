using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class UserSignedInEventHandler : INotificationHandler<UserSignedInEvent>
    {
        private readonly IEventStore _eventStore;

        public UserSignedInEventHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task Handle(UserSignedInEvent notification, CancellationToken cancellationToken)
        {
            await HandleUsingEventStore(notification);
        }

        private async Task HandleUsingEventStore(IEvent @event)
        {
            await _eventStore.Add(@event);
        }
    }
}
