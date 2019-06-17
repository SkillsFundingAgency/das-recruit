using System;

namespace Communication.Types
{
    /// <summary>
    /// an end user that may receive a communication message
    /// </summary>
    public class Participant
    {
        public CommunicationUser User { get; }
        public CommunicationUserPreference Preferences { get; }

        public Participant(CommunicationUser user, CommunicationUserPreference preferences)
        {
            User = user;
            Preferences = preferences;
        }
    }
}
