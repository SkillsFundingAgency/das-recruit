using System;

namespace Communication.Types
{
    /// <summary>
    /// an end user that will receive a communication message
    /// </summary>
    public class Recipient
    {
        public string Email { get; }
        public string Name { get; }
        public string UserType { get; }
        public UserParticipation Participation { get; }

        public Recipient(string email, string name, string userType, UserParticipation participation)
        {
            Email = email;
            Name = name;
            UserType = userType;
            Participation = participation;
        }
    }
}
