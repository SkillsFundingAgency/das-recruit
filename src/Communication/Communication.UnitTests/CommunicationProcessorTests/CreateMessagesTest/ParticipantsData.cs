using Communication.Types;

namespace Communication.UnitTests.CommunicationProcessorTests.CreateMessagesTest
{
    public static class ParticipantsData
    {
        public static Participant OptedIn = new Participant() 
        {
            User = new CommunicationUser{ Name = nameof(OptedIn) },
            Preferences = new CommunicationUserPreference 
            { 
                Channels = DeliveryChannelPreferences.EmailOnly, 
                Scope = NotificationScope.Individual 
            }
        };

        public static Participant OptedOut = new Participant() 
        {
            User = new CommunicationUser{ Name = nameof(OptedOut)},
            Preferences = new CommunicationUserPreference { Channels = DeliveryChannelPreferences.None }
        };

        public static Participant PrimaryOptedOut = new Participant() 
        {
            User = new CommunicationUser{ Name = nameof(PrimaryOptedOut), Participation = UserParticipation.PrimaryUser },
            Preferences = new CommunicationUserPreference { Channels = DeliveryChannelPreferences.None }
        };

        public static Participant SecondaryOptedOut = new Participant() 
        {
            User = new CommunicationUser{ Name = nameof(SecondaryOptedOut), Participation = UserParticipation.SecondaryUser },
            Preferences = new CommunicationUserPreference { Channels = DeliveryChannelPreferences.None }
        };

        public static Participant SecondaryWithOrganisationScope = new Participant() 
        {
            User = new CommunicationUser
            { 
                Participation = UserParticipation.SecondaryUser, 
                Name = nameof(SecondaryWithOrganisationScope) 
            },
            Preferences = new CommunicationUserPreference 
            { 
                Channels = DeliveryChannelPreferences.EmailOnly, 
                Scope = NotificationScope.Organisation
            }
        };

        public static Participant SecondaryWithIndividualScope = new Participant() 
        {
            User = new CommunicationUser
            { 
                Participation = UserParticipation.SecondaryUser, 
                Name = nameof(SecondaryWithIndividualScope) 
            },
            Preferences = new CommunicationUserPreference 
            { 
                Channels = DeliveryChannelPreferences.EmailOnly, 
                Scope = NotificationScope.Individual
            }
        };

        public static Participant PrimaryWithIndividualScope = new Participant() 
        {
            User = new CommunicationUser
            { 
                Participation = UserParticipation.PrimaryUser, 
                Name = nameof(PrimaryWithIndividualScope) 
            },
            Preferences = new CommunicationUserPreference 
            { 
                Channels = DeliveryChannelPreferences.EmailOnly, 
                Scope = NotificationScope.Individual
            }
        };

        public static Participant PrimaryWithOrganisationalScope = new Participant() 
        {
            User = new CommunicationUser
            { 
                Participation = UserParticipation.PrimaryUser, 
                Name = nameof(PrimaryWithOrganisationalScope) 
            },
            Preferences = new CommunicationUserPreference 
            { 
                Channels = DeliveryChannelPreferences.EmailOnly, 
                Scope = NotificationScope.Organisation
            }
        };
    }
}