namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.ManageNotifications;

public class GetUserNotificationPreferencesByIdamsRequest(string idamsId) : IGetApiRequest
{
    public string GetUrl => $"managenotifications/employer/{idamsId}";
}