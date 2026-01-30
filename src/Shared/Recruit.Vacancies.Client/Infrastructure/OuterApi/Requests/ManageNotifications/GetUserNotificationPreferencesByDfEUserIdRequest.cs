namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.ManageNotifications;

public class GetUserNotificationPreferencesByDfEUserIdRequest(string dfeUserId) : IGetApiRequest
{
    public string GetUrl => $"managenotifications/provider/{dfeUserId}";
}