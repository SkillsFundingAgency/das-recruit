namespace Communication.Types
{
    /// <summary>
    /// an end user that may receive a communication message
    /// </summary>
    public class Participant
    {
        public CommunicationUser User { get; set; }
        public CommunicationUserPreference Preferences { get; set; }
    }
}
