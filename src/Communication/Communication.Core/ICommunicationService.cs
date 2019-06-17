using System.Threading.Tasks;
using Communication.Types;

namespace Communication.Core
{
    public interface ICommunicationService
    {
        Task ProcessRequest(CommunicationRequest req);
    }
}