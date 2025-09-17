using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.ManageNotifications;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands.ManageNotifications;

public class UpdateUserNotificationPreferencesCommandHandler(IOuterApiClient outerApiClient) : IRequestHandler<UpdateUserNotificationPreferencesCommand>
{
    public async Task Handle(UpdateUserNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        await outerApiClient.Post(new UpdateUserNotificationPreferencesRequest(request.Id, request.NotificationPreferences));
    }
}