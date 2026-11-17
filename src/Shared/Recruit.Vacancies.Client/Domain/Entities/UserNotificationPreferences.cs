namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class UserNotificationPreferences
{
    public string Id { get; set; }
    public string DfeUserId { get; set; }
    public NotificationTypes NotificationTypes { get; set; }
    public NotificationFrequency? NotificationFrequency { get; set; }
    public NotificationScope? NotificationScope { get; set; }
}