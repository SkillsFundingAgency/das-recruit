using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Messaging;

internal sealed class MediatrMessaging(IMediator mediator) : IMessaging
{
    public async Task<bool> SendStatusCommandAsync(ICommand command)
    {
        var request = command as IRequest<bool>;

        return await mediator.Send(request);
    }

    public async Task SendCommandAsync(ICommand command)
    {
        var request = command as IRequest<Unit>;

        await mediator.Send(request);
    }

    public async Task PublishEvent(IEvent @event)
    {
        var notification = @event as INotification;

        await mediator.Publish(notification);
    }
}