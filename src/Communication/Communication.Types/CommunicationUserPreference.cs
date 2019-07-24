namespace Communication.Types
{
    public struct CommunicationUserPreference
    {
        public DeliveryChannelPreferences Channels { get; set; }
        public DeliveryFrequency Frequency { get; set; }
        public NotificationScope Scope { get; set; }
    }
}
