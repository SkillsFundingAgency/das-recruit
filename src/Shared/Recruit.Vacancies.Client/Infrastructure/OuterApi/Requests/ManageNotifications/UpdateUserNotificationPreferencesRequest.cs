using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.ManageNotifications;

public class UpdateUserNotificationPreferencesRequest(Guid id, NotificationPreferences data) : IPostApiRequest
{
    public string PostUrl => $"managenotifications/{id}";
    public object Data { get; set; } = data;
}