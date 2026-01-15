using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.ManageNotifications;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetEmployerNotificationPreferences;

public class GetEmployerNotificationPreferencesQueryHandler(IOuterApiClient outerApiClient) : IRequestHandler<GetEmployerNotificationPreferencesQuery, GetEmployerNotificationPreferencesQueryResult>
{
    public async Task<GetEmployerNotificationPreferencesQueryResult> Handle(
        GetEmployerNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var result = await outerApiClient.Get<GetUserNotificationPreferencesByIdamsResponse>(new GetUserNotificationPreferencesByIdamsRequest(request.IdamsId));
        return result is null
            ? GetEmployerNotificationPreferencesQueryResult.None
            : new GetEmployerNotificationPreferencesQueryResult
            {
                Id = result.Id,
                IdamsId = result.IdamsId,
                NotificationPreferences = result.NotificationPreferences
            };
    }
}