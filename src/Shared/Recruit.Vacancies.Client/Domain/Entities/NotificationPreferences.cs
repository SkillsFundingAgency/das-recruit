using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class NotificationPreferences
{
    public List<NotificationPreference> EventPreferences { get; set; } = [];
}

public class NotificationPreference
{
    public NotificationTypesEx Event { get; set; }
    public string Method { get; set; }
    public NotificationScopeEx Scope { get; set; }
    public NotificationFrequencyEx Frequency { get; set; }
}

public static class NotificationPreferencesExtensions
{
    public static NotificationPreference GetForEvent(this NotificationPreferences notificationPreferences, NotificationTypesEx eventType)
    {
        return notificationPreferences.EventPreferences.Single(x => x.Event == eventType);
    }
}