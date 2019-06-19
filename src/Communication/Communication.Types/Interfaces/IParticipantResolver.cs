using System.Collections.Generic;
using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface IParticipantResolver
    {
        string ResolverName { get; }
        Task<IEnumerable<CommunicationUser>> GetParticipantsAsync(CommunicationRequest request);
    }
}