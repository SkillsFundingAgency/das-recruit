using System.Collections.Generic;
using System.Linq;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.ParticipantResolverPlugins
{
    public static class ParticipantResolverPluginHelper
    {
        public static IEnumerable<CommunicationUser> ConvertToCommunicationUser(List<User> user, string primaryUserIdamsId)
        {
            return user.Select(u => new CommunicationUser()
            {
                UserId = u.IdamsUserId,
                Email = u.Email,
                Name = u.Name,
                UserType = CommunicationConstants.UserType, 
                Participation = u.IdamsUserId == primaryUserIdamsId ? UserParticipation.PrimaryUser : UserParticipation.SecondaryUser
            });
        }
    }
}