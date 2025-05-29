using System.Collections.Generic;
using System.Linq;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.ParticipantResolverPlugins
{
    public static class ParticipantResolverPluginHelper
    {
        public static IEnumerable<CommunicationUser> ConvertToCommunicationUsers(List<User> users, string primaryUserIdamsId)
        {
            return users.Select(u =>
                new CommunicationUser(
                    userId: u.IdamsUserId,
                    email: u.Email,
                    name: u.Name,
                    userType: CommunicationConstants.UserType,
                    participation: u.IdamsUserId == primaryUserIdamsId ? UserParticipation.PrimaryUser : UserParticipation.SecondaryUser,
                    dfEUserId: u.DfEUserId
                )
            );
        }

        public static DeliveryFrequency GetDeliveryFrequencyPreferenceFromUserFrequencyPreference(this NotificationFrequency? notificationFrequency)
        {
            if (notificationFrequency.HasValue == false)
                return DeliveryFrequency.Default;

            switch (notificationFrequency)
            {
                case NotificationFrequency.Immediately:
                    return DeliveryFrequency.Immediate;
                case NotificationFrequency.Daily:
                    return DeliveryFrequency.Daily;
                case NotificationFrequency.Weekly:
                    return DeliveryFrequency.Weekly;
                default:
                    return DeliveryFrequency.Default;
            }
        }
    }
}