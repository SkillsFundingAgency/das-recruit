using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface IUserPreferencesProvider
    {
        string UserType { get; }
        Task<CommunicationUserPreference> GetUserPreferenceAsync(string requestType, CommunicationUser user);
    }
}