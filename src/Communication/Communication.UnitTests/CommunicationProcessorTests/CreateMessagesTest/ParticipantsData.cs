using Communication.Types;

namespace Communication.UnitTests.CommunicationProcessorTests.CreateMessagesTest
{
    public static class ParticipantsData
    {
        public static Participant OptedIn = new Participant()
        {
            User = new CommunicationUser("userId", "email@email.com", nameof(OptedIn), "userType", UserParticipation.PrimaryUser),
            Preferences = new CommunicationUserPreference
            {
                Channels = DeliveryChannelPreferences.EmailOnly,
                Scope = NotificationScope.Individual
            }
        };

        public static Participant OptedOut = new Participant()
        {
            User = new CommunicationUser("userId", "email@email.com", nameof(OptedOut), "userType", UserParticipation.PrimaryUser),
            Preferences = new CommunicationUserPreference { Channels = DeliveryChannelPreferences.None }
        };

        public static Participant PrimaryOptedOut = new Participant()
        {
            User = new CommunicationUser("userId", "email@email.com", nameof(PrimaryOptedOut), "userType", UserParticipation.PrimaryUser),
            Preferences = new CommunicationUserPreference { Channels = DeliveryChannelPreferences.None }
        };

        public static Participant SecondaryOptedOut = new Participant()
        {
            User = new CommunicationUser("userId", "email@email.com", nameof(SecondaryOptedOut), "userType", UserParticipation.SecondaryUser),
            Preferences = new CommunicationUserPreference { Channels = DeliveryChannelPreferences.None }
        };

        public static Participant SecondaryWithOrganisationScope = new Participant()
        {
            User = new CommunicationUser("userId", "email@email.com", nameof(SecondaryWithOrganisationScope), "userType", UserParticipation.SecondaryUser),
            Preferences = new CommunicationUserPreference
            {
                Channels = DeliveryChannelPreferences.EmailOnly,
                Scope = NotificationScope.Organisation
            }
        };

        public static Participant SecondaryWithIndividualScope = new Participant()
        {
            User = new CommunicationUser("userId", "email@email.com", nameof(SecondaryWithIndividualScope), "userType", UserParticipation.SecondaryUser),
            Preferences = new CommunicationUserPreference
            {
                Channels = DeliveryChannelPreferences.EmailOnly,
                Scope = NotificationScope.Individual
            }
        };

        public static Participant PrimaryWithIndividualScope = new Participant()
        {
            User = new CommunicationUser("userId", "email@email.com", nameof(PrimaryWithIndividualScope), "userType", UserParticipation.PrimaryUser),
            Preferences = new CommunicationUserPreference
            {
                Channels = DeliveryChannelPreferences.EmailOnly,
                Scope = NotificationScope.Individual
            }
        };

        public static Participant PrimaryWithOrganisationalScope = new Participant()
        {
            User = new CommunicationUser("userId", "email@email.com", nameof(PrimaryWithOrganisationalScope), "userType", UserParticipation.PrimaryUser),
            Preferences = new CommunicationUserPreference
            {
                Channels = DeliveryChannelPreferences.EmailOnly,
                Scope = NotificationScope.Organisation
            }
        };
    }
}