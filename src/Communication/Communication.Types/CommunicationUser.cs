namespace Communication.Types
{
    /// <summary>
    /// an end user that may receive a communication message
    /// (depending on their preferences they may be filtered out of the final recipient list)
    /// </summary>
    public class CommunicationUser
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string UserType { get; set; }
        public UserParticipation Participation { get; set; }
    }
}
