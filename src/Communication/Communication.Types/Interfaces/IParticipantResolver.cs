using System.Collections.Generic;
using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface IParticipantResolver
    {
        string ResolverServiceName { get; }
        Task<IEnumerable<CommunicationUser>> GetRecipientsAsync(CommunicationRequest request);
    }
}