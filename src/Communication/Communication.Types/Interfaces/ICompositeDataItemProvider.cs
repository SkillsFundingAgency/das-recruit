using System.Collections.Generic;
using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface ICompositeDataItemProvider
    {
        string ProviderName { get; }
        Task<IEnumerable<CommunicationDataItem>> GetConsolidatedMessageDataItems(CommunicationMessage aggregateMessage, IEnumerable<CommunicationMessage> messages);
    }
}