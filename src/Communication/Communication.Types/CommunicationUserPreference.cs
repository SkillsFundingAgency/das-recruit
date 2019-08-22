namespace Communication.Types
{
    public class CommunicationUserPreference
    {
        public DeliveryChannelPreferences Channels { get; set; }
        public DeliveryFrequency Frequency { get; set; }
        public NotificationScope Scope { get; set; }
    }
}
