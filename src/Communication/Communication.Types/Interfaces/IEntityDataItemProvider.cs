using System.Collections.Generic;
using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface IEntityDataItemProvider
    {
        string ProviderName { get; }
        Task<IEnumerable<CommunicationDataItem>> GetDataItems(object entityId);
    }
}