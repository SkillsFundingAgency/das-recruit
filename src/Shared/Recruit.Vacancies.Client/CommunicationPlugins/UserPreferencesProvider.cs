using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.CommunicationPlugins
{
    public class UserPreferencesProvider : IUserPreferencesProvider
    {
        public string ProviderServiceName => CommunicationConstants.ServiceName;

        public string UserType => throw new System.NotImplementedException();

        public Task<CommunicationUserPreference> GetUserPreference(string requestType, CommunicationUser user)
        {
            throw new System.NotImplementedException();
        }
    }
}