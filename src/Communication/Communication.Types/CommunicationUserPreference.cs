using System;

namespace Communication.Types
{
    public struct CommunicationUserPreference
    {
        public DeliveryChannelPreferences Channels { get; }
        public DeliveryFrequency Frequency { get; }
        public NotificationScope Scope { get; }

        public CommunicationUserPreference(DeliveryChannelPreferences channels, DeliveryFrequency frequency, NotificationScope scope)
        {
            Channels = channels;
            Frequency = frequency;
            Scope = scope;
        }
    }
}
