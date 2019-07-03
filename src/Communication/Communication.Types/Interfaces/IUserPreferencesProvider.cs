using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface IUserPreferencesProvider
    {
        string UserType { get; }
        Task<CommunicationUserPreference> GetUserPreference(string requestType, CommunicationUser user);
    }
}