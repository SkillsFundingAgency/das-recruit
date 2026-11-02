using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Messaging;

internal sealed class MediatrMessaging : IMessaging
{
    private readonly IMediator _mediator;

    public MediatrMessaging(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<bool> SendStatusCommandAsync(ICommand command)
    {
        var request = command as IRequest<bool>;

        return await _mediator.Send(request);
    }

    public async Task SendCommandAsync(ICommand command)
    {
        var request = command as IRequest<Unit>;

        await _mediator.Send(request);
    }

    public async Task PublishEvent(IEvent @event)
    {
        var notification = @event as INotification;

        await _mediator.Publish(notification);
    }
}