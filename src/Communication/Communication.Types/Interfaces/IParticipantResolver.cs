using System.Collections.Generic;
using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface IParticipantResolver
    {
        string ParticipantResolverName { get; }
        Task<IEnumerable<CommunicationUser>> GetParticipantsAsync(CommunicationRequest request);
        Task<IEnumerable<CommunicationMessage>> ValidateParticipantAsync(IEnumerable<CommunicationMessage> messages);
    }
}