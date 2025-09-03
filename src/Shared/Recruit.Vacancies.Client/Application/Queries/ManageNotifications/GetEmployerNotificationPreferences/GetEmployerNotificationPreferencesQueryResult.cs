using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetEmployerNotificationPreferences;

public class GetEmployerNotificationPreferencesQueryResult
{
    public static readonly GetEmployerNotificationPreferencesQueryResult None = new();
    
    public Guid Id { get; set; }
    public string IdamsId { get; set; }
    public NotificationPreferences NotificationPreferences { get; set; }
}