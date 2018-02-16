using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Core.Messaging
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
