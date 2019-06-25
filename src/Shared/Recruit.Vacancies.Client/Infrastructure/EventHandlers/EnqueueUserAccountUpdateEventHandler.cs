using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class EnqueueUserAccountUpdateEventHandler : INotificationHandler<UserSignedInEvent>
    {
        private readonly IRecruitQueueService _queue;
        public EnqueueUserAccountUpdateEventHandler (IRecruitQueueService queueService)
        {
            _queue = queueService;
        }
        public Task Handle (UserSignedInEvent notification, CancellationToken cancellationToken)
        {
            return _queue.AddMessageAsync(new UpdateUserAccountQueueMessage { IdamsUserId = notification.IdamsUserId });
        }
    }
}