using Esfa.Recruit.Storage.Client.Domain.Messaging;
using MediatR;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Infrastructure.Messaging
{
    public class MediatrMessaging : IMessaging
    {
        private readonly IMediator _mediator;

        public MediatrMessaging(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task SendCommandAsync(ICommand command)
        {
            var request = command as IRequest;

            await _mediator.Send(request);
        }
    }
}
