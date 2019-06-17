using System.Collections.Generic;
using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface IParticipantResolver
    {
        string ResolverName { get; }
        Task<IEnumerable<CommunicationUser>> GetParticipants(ICommunicationRequest request, IEnumerable<CommunicationDataItem> dataItems);
        Task<IEnumerable<CommunicationMessage>> ValidateParticipant(IEnumerable<CommunicationMessage> messagesToBeAggregated);
    }
}