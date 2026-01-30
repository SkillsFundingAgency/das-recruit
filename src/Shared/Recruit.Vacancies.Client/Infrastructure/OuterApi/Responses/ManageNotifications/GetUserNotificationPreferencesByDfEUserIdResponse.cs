using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.ManageNotifications;

public class GetUserNotificationPreferencesByDfEUserIdResponse
{
    public Guid Id { get; set; }
    public string DfEUserId { get; set; }
    public NotificationPreferences NotificationPreferences { get; set; }
}