using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class EnqueueUserAccountUpdateEventHandler : INotificationHandler<UserSignedInEvent> 
    {
        private readonly IQueueService _queueService;
        public EnqueueUserAccountUpdateEventHandler (IQueueService queueService) 
        {
            _queueService = queueService;
        }
        public Task Handle (UserSignedInEvent notification, CancellationToken cancellationToken) 
        {
            return _queueService.AddMessageAsync(new UpdateUserAccountQueueMessage { IdamsUserId = notification.IdamsUserId });
        }
    }
}