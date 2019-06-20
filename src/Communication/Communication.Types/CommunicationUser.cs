namespace Communication.Types
{
    /// <summary>
    /// an end user that may receive a communication message
    /// (depending on their preferences they may be filtered out of the final recipient list)
    /// </summary>
    public class CommunicationUser
    {
        public string Email { get; }
        public string Name { get; }
        public string UserType { get; }
        public UserParticipation Participation { get; }

        public CommunicationUser(string email, string name, string userType, UserParticipation participation)
        {
            Email = email;
            Name = name;
            UserType = userType;
            Participation = participation;
        }
    }
}
