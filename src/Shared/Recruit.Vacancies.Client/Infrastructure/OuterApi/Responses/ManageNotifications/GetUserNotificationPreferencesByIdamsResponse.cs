using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.ManageNotifications;

public class GetUserNotificationPreferencesByIdamsResponse
{
    public Guid Id { get; set; }
    public string IdamsId { get; set; }
    public NotificationPreferences NotificationPreferences { get; set; }
}