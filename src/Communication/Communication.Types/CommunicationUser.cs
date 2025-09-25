using MongoDB.Bson.Serialization.Attributes;

namespace Communication.Types
{
    /// <summary>
    /// an end user that may receive a communication message
    /// (depending on their preferences they may be filtered out of the final recipient list)
    /// </summary>
    public class CommunicationUser
    {
        public string UserId { get; }
        public string Email { get; }
        public string Name { get; }
        /// This will be used to resolve UserPreferencesProvider
        /// This has to be a unique value across systems
        /// example values: VacancyServices.Recruit.Employer, VacancyServices.Faa.Candidates
        public string UserType { get; }
        public UserParticipation Participation { get; }
        [BsonDefaultValue("")]
        public string DfEUserId { get; }

        public CommunicationUser(string userId, string email, string name, string userType, UserParticipation participation, string dfEUserId)
        {
            UserId = userId;
            Email = email;
            Name = name;
            UserType = userType;
            Participation = participation;
            DfEUserId = dfEUserId;
        }
    }
}
