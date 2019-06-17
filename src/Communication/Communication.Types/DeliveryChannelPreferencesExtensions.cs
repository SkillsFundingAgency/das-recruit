using System;
using System.Collections.Generic;
using System.Linq;

namespace Communication.Types
{
    internal static class DeliveryChannelPreferencesExtensions
    {
        internal static IEnumerable<DeliveryChannel> ToDeliveryChannels(this DeliveryChannelPreferences preferences)
        {
            switch (preferences)
            {
                case DeliveryChannelPreferences.EmailOnly:
                    return new[] {DeliveryChannel.Email};
                case DeliveryChannelPreferences.SmsOnly:
                    return new[] {DeliveryChannel.Sms};
                case DeliveryChannelPreferences.EmailAndSms:
                    return new[] {DeliveryChannel.Email, DeliveryChannel.Sms};
                default:
                    return Enumerable.Empty<DeliveryChannel>();
            }
        }
    }
}
