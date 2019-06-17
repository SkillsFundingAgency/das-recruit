using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;

namespace Communication.Core
{
    public interface ICommunicationProcessor
    {
        Task<IEnumerable<CommunicationMessage>> CreateMessagesAsync(CommunicationRequest request);
    }
}