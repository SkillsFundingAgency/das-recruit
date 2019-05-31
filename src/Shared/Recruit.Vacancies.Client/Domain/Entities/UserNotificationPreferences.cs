using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class UserNotificationPreferences
    {
        public Guid Id { get; set; }
        public NotificationTypes NotificationTypes { get; set; }
        public NotificationFrequency? NotificationFrequency { get; set; }
        public NotificationScope? NotificationScope { get; set; }
    }
}