using System;

namespace Communication.Types
{
    /// <summary>
    /// for a particular communication message, this differentiates between a message that
    /// relates directly to the user (eg. if they created a vacancy that has been rejected)
    /// vs. where the user is being notified because they are interested in all communications
    /// for the whole organisation
    /// </summary>
    public enum Participation
    {
        PrimaryUser,
        SecondaryUser
    }
}
