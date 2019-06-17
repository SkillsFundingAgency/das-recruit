using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface IUserPreferencesProvider
    {
        string ProviderServiceName { get; }
        string UserType { get; }
        Task<CommunicationUserPreference> GetUserPreference(string requestType, CommunicationUser user);
    }
}