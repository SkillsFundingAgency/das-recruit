using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.ManageNotifications;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetProviderNotificationPreferences;

public class GetProviderNotificationPreferencesQueryHandler(IOuterApiClient outerApiClient) : IRequestHandler<GetProviderNotificationPreferencesQuery, GetProviderNotificationPreferencesQueryResult>
{
    public async Task<GetProviderNotificationPreferencesQueryResult> Handle(GetProviderNotificationPreferencesQuery query, CancellationToken cancellationToken)
    {
        var result = await outerApiClient.Get<GetUserNotificationPreferencesByDfEUserIdResponse>(new GetUserNotificationPreferencesByDfEUserIdRequest(query.DfEUserId));
        return result is null
            ? GetProviderNotificationPreferencesQueryResult.None
            : new GetProviderNotificationPreferencesQueryResult
            {
                Id = result.Id,
                DfEUserId = result.DfEUserId,
                NotificationPreferences = result.NotificationPreferences
            };
    }
}