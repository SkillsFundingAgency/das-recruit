using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetProviderNotificationPreferences;

public record GetProviderNotificationPreferencesQueryResult
{
    public static readonly GetProviderNotificationPreferencesQueryResult None = new();
    
    public Guid Id { get; set; }
    public string DfEUserId { get; set; }
    public NotificationPreferences NotificationPreferences { get; set; }
}